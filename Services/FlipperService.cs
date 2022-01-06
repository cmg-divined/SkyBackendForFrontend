using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coflnet.Sky;
using Coflnet.Sky.Commands;
using Coflnet.Sky.Commands.Helper;
using Coflnet.Sky.Commands.Shared;
using Confluent.Kafka;
using OpenTracing.Propagation;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;
using Newtonsoft.Json;

namespace hypixel
{

    /// <summary>
    /// Frontendfacing methods for the flipper
    /// </summary>
    public class FlipperService
    {
        public static FlipperService Instance = new FlipperService();

        private ConcurrentDictionary<long, FlipConWrapper> Subs = new ConcurrentDictionary<long, FlipConWrapper>();
        private ConcurrentDictionary<long, FlipConWrapper> SlowSubs = new ConcurrentDictionary<long, FlipConWrapper>();
        private ConcurrentDictionary<long, FlipConWrapper> SuperSubs = new ConcurrentDictionary<long, FlipConWrapper>();
        public ConcurrentQueue<FlipInstance> Flipps = new ConcurrentQueue<FlipInstance>();
        private ConcurrentQueue<FlipInstance> SlowFlips = new ConcurrentQueue<FlipInstance>();


        /// <summary>
        /// Wherether or not a given <see cref="SaveAuction.UId"/> was a flip or not
        /// </summary>
        private ConcurrentDictionary<long, bool> FlipIdLookup = new ConcurrentDictionary<long, bool>();
        public static readonly string ConsumeTopic = SimplerConfig.Config.Instance["TOPICS:FLIP"];
        public static readonly string LowPriceConsumeTopic = SimplerConfig.Config.Instance["TOPICS:LOW_PRICED"];
        public static readonly string SettingsTopic = SimplerConfig.Config.Instance["TOPICS:SETTINGS_CHANGE"];
        private static ProducerConfig producerConfig = new ProducerConfig { BootstrapServers = SimplerConfig.Config.Instance["KAFKA_HOST"] };

        private const string FoundFlippsKey = "foundFlipps";
        public int PremiumUserCount => Subs.Select(s => s.Value.Connection.UserId).Distinct().Count();

        static Prometheus.Histogram runtroughTime = Prometheus.Metrics.CreateHistogram("sky_commands_auction_to_flip_seconds", "Represents the time in seconds taken from loading the auction to sendingthe flip. (should be close to 0)",
            new Prometheus.HistogramConfiguration()
            {
                Buckets = Prometheus.Histogram.LinearBuckets(start: 1, width: 2, count: 10)
            });

        /// <summary>
        /// Special load burst queue that will send out 5 flips at load
        /// </summary>
        private Queue<FlipInstance> LoadBurst = new Queue<FlipInstance>();
        private ConcurrentDictionary<long, DateTime> SoldAuctions = new ConcurrentDictionary<long, DateTime>();
        static RestClient SkyFlipperHost = new RestClient("http://" + SimplerConfig.Config.Instance["SKYFLIPPER_HOST"]);

        public event Action<SettingsChange> OnSettingsChange;

        private async Task TryLoadFromCache()
        {
            if (Flipps.Count == 0)
            {
                // try to get from redis

                var fromCache = await CacheService.Instance.GetFromRedis<ConcurrentQueue<FlipInstance>>(FoundFlippsKey);
                if (fromCache != default(ConcurrentQueue<FlipInstance>))
                {
                    Flipps = fromCache;
                    foreach (var item in Flipps)
                    {
                        FlipIdLookup[item.UId] = true;
                    }
                }
            }
        }


        public async Task<DeliveryResult<string, SettingsChange>> UpdateSettings(SettingsChange settings)
        {
            var cacheKey = "uflipset" + settings.UserId;
            var serializer = SerializerFactory.GetSerializer<SettingsChange>();
            /* var stored = await CacheService.Instance.GetFromRedis<SettingsChange>(cacheKey);
            //if(serializer.Serialize(settings,default).SequenceEqual(serializer.Serialize(stored,default)))
            //    return null; */
            using (var p = new ProducerBuilder<string, SettingsChange>(producerConfig).SetValueSerializer(serializer).Build())
            {
                var produceTask = p.ProduceAsync(SettingsTopic, new Message<string, SettingsChange> { Value = settings });
                await CacheService.Instance.SaveInRedis(cacheKey, settings, TimeSpan.FromDays(5));
                if (settings.LongConIds.Any())
                    await CacheService.Instance.SaveInRedis(settings.LongConIds.LastOrDefault().ToString(), settings);

                return await produceTask;
            }
        }

        public void AddConnection(IFlipConnection connection, bool sendHistory = true)
        {
            var con = new FlipConWrapper(connection);
            Subs.AddOrUpdate(con.Connection.Id, cid => con, (cid, oldMId) => con);
            var toSendFlips = Flipps.Reverse().Take(25);
            if (sendHistory)
                SendFlipHistory(connection, toSendFlips, 0);
            RemoveNonConnection(con.Connection);
            Task.Run(con.Work);
        }

        public void AddConnectionPlus(IFlipConnection connection, bool sendHistory = true)
        {
            var con = new FlipConWrapper(connection);
            RemoveConnection(con.Connection);
            SuperSubs.AddOrUpdate(con.Connection.Id, cid => con, (cid, oldMId) => con);
            var toSendFlips = Flipps.Reverse().Take(25);
            if (sendHistory)
                SendFlipHistory(connection, toSendFlips, 0);
            RemoveNonConnection(con.Connection);
            Task.Run(con.Work);
        }

        public void AddNonConnection(IFlipConnection connection, bool sendHistory = true)
        {
            var con = new FlipConWrapper(connection);
            SlowSubs.AddOrUpdate(connection.Id, cid => con, (cid, oldMId) => con);
            if (!sendHistory)
                return;
            SendFlipHistory(connection, LoadBurst, 0);
            if (SlowSubs.Count % 10 == 0)
                Console.WriteLine("Added new con " + SlowSubs.Count);

        }

        private void RemoveNonConnection(IFlipConnection con)
        {
            Unsubscribe(SlowSubs, con.Id);
        }

        public void RemoveConnection(IFlipConnection con)
        {
            Unsubscribe(Subs, con.Id);
            Unsubscribe(SuperSubs, con.Id);
            RemoveNonConnection(con);
        }




        private static void SendFlipHistory(IFlipConnection con, IEnumerable<FlipInstance> toSendFlips, int delay = 5000)
        {
            Task.Run(async () =>
            {
                try
                {

                    foreach (var item in toSendFlips)
                    {
                        await con.SendFlip(item);

                        await Task.Delay(delay);
                    }
                }
                catch (Exception e)
                {
                    dev.Logger.Instance.Error(e, "sending history");
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Tell the flipper that an auction was sold
        /// </summary>
        /// <param name="auction"></param>
        public void AuctionSold(SaveAuction auction)
        {
            if (!FlipIdLookup.ContainsKey(auction.UId))
                return;
            SoldAuctions[auction.UId] = auction.End;
            var auctionUUid = auction.Uuid;
            Task.Run(() => NotifySubsInactiveAuction(auctionUUid));
        }

        private async Task NotifySubsInactiveAuction(string auctionUUid)
        {
            var inacive = new List<long>();
            foreach (var item in Subs)
            {
                if (!await item.Value.Connection.SendSold(auctionUUid))
                    inacive.Add(item.Key);
            }
            foreach (var item in SlowSubs)
            {
                if (!await item.Value.Connection.SendSold(auctionUUid))
                    inacive.Add(item.Key);
            }

            foreach (var item in inacive)
            {
                Unsubscribe(SlowSubs, item);
                Unsubscribe(Subs, item);
            }
        }

        public static FlipInstance LowPriceToFlip(LowPricedAuction flip)
        {
            var flipIntance = new FlipInstance()
            {
                LastKnownCost = (int)flip.Auction.StartingBid,
                Auction = flip.Auction,
                MedianPrice = flip.TargetPrice,
                Uuid = flip.Auction.Uuid,
                Bin = flip.Auction.Bin,
                Name = flip.Auction.ItemName,
                Interesting = PropertiesSelector.GetProperties(flip.Auction).OrderByDescending(a => a.Rating).Select(a => a.Value).ToList(),
                Tag = flip.Auction.Tag,
                Volume = flip.DailyVolume,
                Rarity = flip.Auction.Tier,
                Finder = flip.Finder,
                LowestBin = flip.Finder == LowPricedAuction.FinderType.SNIPER ? flip.TargetPrice : 0,
                Context = flip.AdditionalProps == null || flip.AdditionalProps.Count == 0 ? new Dictionary<string, string>() : new Dictionary<string, string>(flip.AdditionalProps)
            };
            return flipIntance;
        }

        public static async Task FillVisibilityProbs(FlipInstance flip, FlipSettings settings)
        {
            if (settings == null || settings.Visibility == null)
                return;
            if (settings.Visibility.Seller && flip.SellerName == null)
                flip.SellerName = (await DiHandler.ServiceProvider.GetRequiredService<Coflnet.Sky.PlayerName.Client.Api.PlayerNameApi>()
                    .PlayerNameNameUuidGetAsync(flip.Auction.AuctioneerId))?.Trim('"');

            if (flip.LowestBin == 0 && (settings.Visibility.LowestBin || settings.Visibility.SecondLowestBin || settings.BasedOnLBin))
            {
                var lowestBin = await GetLowestBin(flip.Auction);
                flip.LowestBin = lowestBin?.FirstOrDefault()?.Price;
                flip.SecondLowestBin = lowestBin?.Count >= 2 ? lowestBin[1].Price : 0L;
            }
        }

        public static async Task<List<ItemPrices.AuctionPreview>> GetLowestBin(SaveAuction auction)
        {
            var filters = new Dictionary<string, string>();
            var ulti = auction.Enchantments.Where(e => Coflnet.Sky.Constants.RelevantEnchants.Where(rel => rel.Type == e.Type && rel.Level <= e.Level).Any()).FirstOrDefault();
            if (ulti != null)
            {
                filters["Enchantment"] = ulti.Type.ToString();
                filters["EnchantLvl"] = ulti.Level.ToString();
            }
            if (Coflnet.Sky.Constants.RelevantReforges.Contains(auction.Reforge))
            {
                filters["Reforge"] = auction.Reforge.ToString();
            }
            filters["Rarity"] = auction.Tier.ToString();

            var exactLowestTask = ItemPrices.GetLowestBin(auction.Tag, filters);
            List<ItemPrices.AuctionPreview> lowestBin = await ItemPrices.GetLowestBin(auction.Tag, auction.Tier);
            var exactLowest = await exactLowestTask;
            if (exactLowest?.Count > 1)
                return exactLowest;
            return lowestBin;
        }

        /// <summary>
        /// Auction is no longer active for some reason
        /// </summary>
        /// <param name="uuid"></param>
        public async Task AuctionInactive(string uuid)
        {
            await NotifySubsInactiveAuction(uuid);
            var uid = AuctionService.Instance.GetId(uuid);
            SoldAuctions[uid] = DateTime.Now;
        }



        /// <summary>
        /// Sends out new flips based on tier.
        /// (active on the light client)
        /// </summary>
        /// <param name="flip"></param>
        private async Task DeliverFlip(FlipInstance flip)
        {
            if (flip.Auction?.Start < DateTime.Now - TimeSpan.FromMinutes(3) && flip.Auction?.Start != default)
                return; // skip old flips
            runtroughTime.Observe((DateTime.Now - flip.Auction.FindTime).TotalSeconds);
            var tracer = OpenTracing.Util.GlobalTracer.Instance;
            var span = OpenTracing.Util.GlobalTracer.Instance.BuildSpan("SendFlip");
            if (flip.Auction.TraceContext != null)
                span = span.AsChildOf(tracer.Extract(BuiltinFormats.TextMap, flip.Auction.TraceContext));
            using var scope = span.StartActive();

            flip.Finder = LowPricedAuction.FinderType.FLIPPER;
            await NotifyAll(flip, Subs);
            PrepareSlow(flip);
        }

        private void PrepareSlow(FlipInstance flip)
        {
            if (FlipIdLookup.ContainsKey(flip.UId))
                return;
            SlowFlips.Enqueue(flip);
            Flipps.Enqueue(flip);
            FlipIdLookup[flip.UId] = true;
            if (Flipps.Count > 1500)
            {
                if (Flipps.TryDequeue(out FlipInstance result))
                {
                    FlipIdLookup.Remove(result.UId, out bool value);
                }
            }
        }

        public Task DeliverLowPricedAuction(LowPricedAuction flip)
        {
            if (flip.Auction.Context != null)
                flip.Auction.Context["crec"] = (DateTime.Now - flip.Auction.FindTime).ToString();
            var tracer = OpenTracing.Util.GlobalTracer.Instance;
            var span = OpenTracing.Util.GlobalTracer.Instance.BuildSpan("DeliverFlip");
            //if (flip.Auction.TraceContext != null)
            //    span = span.AsChildOf(tracer.Extract(BuiltinFormats.TextMap, flip.Auction.TraceContext));
            using var scope = span.StartActive();
            var time = (DateTime.Now - flip.Auction.FindTime).TotalSeconds;
            if (time > 5)
                scope.Span.SetTag("slow", true);

            if (flip.Auction != null && flip.Auction.NBTLookup == null)
                flip.Auction.NBTLookup = NBT.CreateLookup(flip.Auction);
            foreach (var item in SuperSubs)
            {
                item.Value.AddLowPriced(flip);
            }

            foreach (var item in Subs)
            {
                item.Value.AddLowPriced(flip);
                scope.Span.Log("sent " + item.Value.Connection.UserId);
            }
            PrepareSlow(LowPriceToFlip(flip));
            return Task.CompletedTask;
        }


        private static async Task NotifyAll(FlipInstance flip, ConcurrentDictionary<long, FlipConWrapper> subscribers)
        {
            if (flip.Auction != null && flip.Auction.NBTLookup == null)
                flip.Auction.NBTLookup = NBT.CreateLookup(flip.Auction);
            foreach (var item in subscribers.Keys)
            {
                try
                {
                    if (!subscribers.TryGetValue(item, out FlipConWrapper connection) || !await connection.SendFlip(flip))
                        Unsubscribe(subscribers, item);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to send flip {e.Message} {e.StackTrace}");
                    Unsubscribe(subscribers, item);
                }
            }
        }

        private static void Unsubscribe(ConcurrentDictionary<long, FlipConWrapper> subscribers, long item)
        {
            if (subscribers.TryRemove(item, out FlipConWrapper value))
                value.Stop();
        }

        public async Task ProcessSlowQueue()
        {
            try
            {
                if (SlowFlips.TryDequeue(out FlipInstance flip))
                {
                    if (SoldAuctions.ContainsKey(flip.UId))
                        flip.Sold = true;
                    await NotifyAll(flip, SlowSubs);
                    if (flip.Uuid[0] == 'a')
                        Console.Write("sf+" + SlowSubs.Count);
                    LoadBurst.Enqueue(flip);
                    if (LoadBurst.Count > 5)
                        LoadBurst.Dequeue();
                }

                await Task.Delay(DelayTimeFor(SlowFlips.Count) * 4 / 5);
            }
            catch (Exception e)
            {
                dev.Logger.Instance.Error(e, "slow queue processor");
            }
        }

        ConsumerConfig consumerConf = new ConsumerConfig
        {
            GroupId = System.Net.Dns.GetHostName(),
            BootstrapServers = Program.KafkaHost,
            AutoOffsetReset = AutoOffsetReset.Latest
        };


        public Task ListentoUnavailableTopics()
        {
            Console.WriteLine("listening to unavailibily topics");
            string[] topics = new string[] {
                SimplerConfig.Config.Instance["TOPICS:MISSING_AUCTION"],
                SimplerConfig.Config.Instance["TOPICS:SOLD_AUCTION"],
                SimplerConfig.Config.Instance["TOPICS:AUCTION_ENDED"] };
            return ConsumeBatch<SaveAuction>(topics, AuctionSold);
        }

        public async Task ListenToNewFlips()
        {

            await TryLoadFromCache();
            string[] topics = new string[] { ConsumeTopic };

            Console.WriteLine("starting to listen for new auctions via topic " + ConsumeTopic);
            await ConsumeBatch<FlipInstance>(topics, flip =>
            {
                if (flip.MedianPrice - flip.LastKnownCost < 50_000)
                    return;
                Task.Run(async () =>
                {
                    try
                    {
                        await DeliverFlip(flip);
                    }
                    catch (Exception e)
                    {
                        dev.Logger.Instance.Error(e, "delivering flip");
                    }
                });
            });
            Console.WriteLine("ended listening");
        }

        public Task ListenForSettingsChange()
        {
            string[] topics = new string[] { SettingsTopic };

            Console.WriteLine("starting to listen for config changes topic " + SettingsTopic);
            return ConsumeBatch<SettingsChange>(topics, UpdateSettingsInternal);
        }


        public async Task ListenToLowPriced()
        {
            string[] topics = new string[] { LowPriceConsumeTopic };

            await ConsumeBatch<LowPricedAuction>(topics, flip =>
            {
                if (flip.Auction.Start < DateTime.Now - TimeSpan.FromMinutes(3))
                    return;

                if (flip.TargetPrice - flip.Auction.StartingBid < 50_000)
                    return;
                var time = (DateTime.Now - flip.Auction.FindTime).TotalSeconds;
                runtroughTime.Observe(time);

                try
                {
                    DeliverLowPricedAuction(flip);
                }
                catch (Exception e)
                {
                    dev.Logger.Instance.Error(e, "delivering low priced auction");
                }
            });
        }

        protected virtual void UpdateSettingsInternal(SettingsChange settings)
        {
            foreach (var item in settings.LongConIds)
            {
                if (SlowSubs.TryGetValue(item, out FlipConWrapper con)
                    || Subs.TryGetValue(item, out con)
                    || SuperSubs.TryGetValue(item, out con))
                {
                    con.Connection.UpdateSettings(settings);
                }
            }
            OnSettingsChange?.Invoke(settings);
        }

        private async Task ConsumeBatch<T>(string[] topics, Action<T> work, int batchSize = 10)
        {
            using (var c = new ConsumerBuilder<Ignore, T>(consumerConf).SetValueDeserializer(SerializerFactory.GetDeserializer<T>()).Build())
            {
                c.Subscribe(topics);
                try
                {
                    var batch = new List<TopicPartitionOffset>();
                    Console.WriteLine("subscribed to " + string.Join(",", topics));
                    while (true)
                    {
                        try
                        {
                            var cr = c.Consume(30000);
                            if (cr == null)
                            {
                                await Task.Delay(10);
                                continue;
                            }
                            if (cr.TopicPartitionOffset.Offset % 200 == 0)
                                Console.WriteLine($"consumed {cr.TopicPartitionOffset.Topic} {cr.TopicPartitionOffset.Offset}");
                            work(cr.Message.Value);
                            batch.Add(cr.TopicPartitionOffset);
                            while (batch.Count <= batchSize)
                            {
                                cr = c.Consume(TimeSpan.Zero);
                                if (cr == null)
                                {
                                    break;
                                }
                                batch.Add(cr.TopicPartitionOffset);
                                work(cr.Message.Value);
                            }
                        }
                        catch (ConsumeException e)
                        {
                            dev.Logger.Instance.Error(e, "flipper consume batch " + topics[0]);
                        }
                        if (batch.Count > batchSize)
                        {
                            // tell kafka that we stored the batch
                            c.Commit(batch);
                            batch.Clear();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            }
        }

        public static int DelayTimeFor(int queueSize)
        {
            return (int)Math.Min((TimeSpan.FromMinutes(5) / (Math.Max(queueSize, 1))).TotalMilliseconds, 10000);
        }

        /// <summary>
        /// Removes old <see cref="SoldAuctions"/>
        /// </summary>
        private void ClearSoldBuffer()
        {
            var toRemove = new List<long>();
            var oldestTime = DateTime.Now - TimeSpan.FromMinutes(10);
            foreach (var item in SoldAuctions)
            {
                if (item.Value < oldestTime)
                    toRemove.Add(item.Key);
            }
            foreach (var item in toRemove)
            {
                SoldAuctions.TryRemove(item, out DateTime deleted);
            }
        }

        public async Task<List<SaveAuction>> GetReferences(string uuid)
        {
            var response = await SkyFlipperHost.ExecuteAsync(new RestRequest("flip/{uuid}/based").AddParameter("uuid", uuid, ParameterType.UrlSegment));
            var result = JsonConvert.DeserializeObject<List<SaveAuction>>(response.Content);
            return result;
        }
    }
}
