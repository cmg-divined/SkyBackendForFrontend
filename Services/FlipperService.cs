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
using Coflnet.Sky.Core;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace Coflnet.Sky.Commands.Shared
{

    /// <summary>
    /// Frontendfacing methods for the flipper
    /// </summary>
    public class FlipperService : BackgroundService
    {
        public static FlipperService Instance = new FlipperService();

        private ConcurrentDictionary<long, FlipConWrapper> Subs = new ConcurrentDictionary<long, FlipConWrapper>();
        private ConcurrentDictionary<long, FlipConWrapper> SlowSubs = new ConcurrentDictionary<long, FlipConWrapper>();
        private ConcurrentDictionary<long, FlipConWrapper> StarterSubs = new ConcurrentDictionary<long, FlipConWrapper>();
        private ConcurrentDictionary<long, FlipConWrapper> SuperSubs = new ConcurrentDictionary<long, FlipConWrapper>();
        public ConcurrentQueue<FlipInstance> Flipps = new ConcurrentQueue<FlipInstance>();
        private ConcurrentQueue<FlipInstance> SlowFlips = new ConcurrentQueue<FlipInstance>();
        private ConcurrentQueue<LowPricedAuction> StarterFlips = new();
        public ServerFilterSumary FilterSumary { get; private set; } = new();


        /// <summary>
        /// Wherether or not a given <see cref="SaveAuction.UId"/> was a flip or not
        /// </summary>
        private ConcurrentDictionary<long, bool> FlipIdLookup = new ConcurrentDictionary<long, bool>();
        public static readonly string ConsumeTopic = SimplerConfig.Config.Instance["TOPICS:FLIP"];
        public static readonly string LowPriceConsumeTopic = SimplerConfig.Config.Instance["TOPICS:LOW_PRICED"];
        public static readonly string AuctionConsumeTopic = SimplerConfig.Config.Instance["TOPICS:NEW_AUCTION"];
        public static readonly string SettingsTopic = SimplerConfig.Config.Instance["TOPICS:SETTINGS_CHANGE"];

        private const string FoundFlippsKey = "foundFlipps";
        public int PremiumUserCount => Subs.Count() + SuperSubs.Count();

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


        public void AddConnectionPlus(IFlipConnection connection, bool sendHistory = true)
        {
            if (SuperSubs.ContainsKey(connection.Id))
                return;
            var con = new FlipConWrapper(connection);
            RemoveConnection(con.Connection);
            SuperSubs.AddOrUpdate(con.Connection.Id, cid => con, (cid, oldMId) => con);
            var toSendFlips = Flipps.Reverse().Take(25);
            if (sendHistory)
                SendFlipHistory(connection, toSendFlips, 0);
            RemoveNonConnection(con.Connection);
            Task.Run(con.Work);
        }

        public void AddConnection(IFlipConnection connection, bool sendHistory = true)
        {
            if (Subs.ContainsKey(connection.Id))
                return;
            var con = new FlipConWrapper(connection);
            Subs.AddOrUpdate(con.Connection.Id, cid => con, (cid, oldMId) => con);
            var toSendFlips = Flipps.Reverse().Take(25);
            if (sendHistory)
                SendFlipHistory(connection, toSendFlips, 0);
            RemoveNonConnection(con.Connection);
            Task.Run(con.Work);
        }

        public void AddStarterConnection(IFlipConnection connection, bool sendHistory = true)
        {
            var con = new FlipConWrapper(connection);
            StarterSubs.AddOrUpdate(connection.Id, cid => con, (cid, oldMId) => con);
            if (sendHistory)
                SendFlipHistory(connection, LoadBurst, 0);
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
            UpdateFilterSumaries();
        }

        private void RemoveNonConnection(IFlipConnection con)
        {
            Unsubscribe(SlowSubs, con.Id);
            UpdateFilterSumaries();
        }

        public void RemoveConnection(IFlipConnection con)
        {
            Unsubscribe(Subs, con.Id);
            Unsubscribe(SuperSubs, con.Id);
            Unsubscribe(StarterSubs, con.Id);
            RemoveNonConnection(con);
        }




        private static void SendFlipHistory(IFlipConnection con, IEnumerable<FlipInstance> toSendFlips, int delay = 5000)
        {
            Task.Run(async () =>
            {
                try
                {
                    foreach (var item in toSendFlips.ToList())
                    {
                        await con.SendFlip(item);

                        await Task.Delay(delay).ConfigureAwait(false);
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
                Auction = flip.Auction,
                MedianPrice = flip.Finder == LowPricedAuction.FinderType.SNIPER && flip.AdditionalProps.TryGetValue("mVal", out string mVal) ? long.Parse(mVal) : flip.TargetPrice,
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
            var timeOut = new System.Threading.CancellationTokenSource(5000);
            if (settings.Visibility.Seller && flip.SellerName == null)
            {
                try
                {
                    flip.SellerName = (await DiHandler.ServiceProvider.GetRequiredService<Coflnet.Sky.PlayerName.Client.Api.PlayerNameApi>()
                                    .PlayerNameNameUuidGetAsync(flip.Auction.AuctioneerId, 0, timeOut.Token).ConfigureAwait(false))?.Trim('"');
                }
                catch (TaskCanceledException)
                {
                    flip.SellerName = $"not-found";
                }
            }


            if (flip.LowestBin == 0 && (settings.Visibility.LowestBin || settings.Visibility.SecondLowestBin || settings.BasedOnLBin))
            {
                var lowestBin = await GetLowestBin(flip.Auction).ConfigureAwait(false);
                flip.LowestBin = lowestBin?.FirstOrDefault()?.Price;
                flip.SecondLowestBin = lowestBin?.Count >= 2 ? lowestBin[1].Price : 0L;
                if (settings.BasedOnLBin)
                    flip.MedianPrice = flip.LowestBin ?? flip.MedianPrice;
            }
        }

        public static async Task<List<ItemPrices.AuctionPreview>> GetLowestBin(SaveAuction auction)
        {
            var filters = new Dictionary<string, string>();
            var ulti = auction.Enchantments.Where(e => Coflnet.Sky.Core.Constants.RelevantEnchants.Where(rel => rel.Type == e.Type && rel.Level <= e.Level).Any()).FirstOrDefault();
            if (ulti != null)
            {
                filters["Enchantment"] = ulti.Type.ToString();
                filters["EnchantLvl"] = ulti.Level.ToString();
            }
            if (Coflnet.Sky.Core.Constants.RelevantReforges.Contains(auction.Reforge))
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
            SoldAuctions[uid] = DateTime.UtcNow;
        }



        /// <summary>
        /// Sends out new flips based on tier.
        /// (active on the light client)
        /// </summary>
        /// <param name="flip"></param>
        private async Task DeliverFlip(FlipInstance flip)
        {
            if (flip.Auction?.Start < DateTime.UtcNow - TimeSpan.FromMinutes(3) && flip.Auction?.Start != default)
                return; // skip old flips
            runtroughTime.Observe((DateTime.UtcNow - flip.Auction.FindTime).TotalSeconds);
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

        public async Task DeliverLowPricedAuctions(IEnumerable<LowPricedAuction> flips)
        {
            Console.WriteLine("queuing batch of " + flips.Count());
            await Parallel.ForEachAsync(flips, async (item, s) =>
            {
                await DeliverLowPricedAuction(item);
            });
        }


        public async Task DeliverLowPricedAuction(LowPricedAuction flip, AccountTier minAccountTier = AccountTier.PREMIUM)
        {
            if (flip.Auction.Context != null)
                flip.Auction.Context["crec"] = (DateTime.UtcNow - flip.Auction.FindTime).ToString();
            var tracer = OpenTracing.Util.GlobalTracer.Instance;
            var span = OpenTracing.Util.GlobalTracer.Instance.BuildSpan("DeliverFlip");
            //if (flip.Auction.TraceContext != null)
            //    span = span.AsChildOf(tracer.Extract(BuiltinFormats.TextMap, flip.Auction.TraceContext));
            using var scope = span.StartActive();
            var time = (DateTime.UtcNow - flip.Auction.FindTime).TotalSeconds;
            if (time > 3)
                scope.Span.SetTag("slow", true).SetTag("uuid", flip.Auction.Uuid);

            if (flip.Auction != null && flip.Auction.NBTLookup == null)
                flip.Auction.NBTLookup = NBT.CreateLookup(flip.Auction);

            if (flip.Auction.Context != null)
                flip.Auction.Context["csh"] = (DateTime.UtcNow - flip.Auction.FindTime).ToString();
            foreach (var item in SuperSubs)
            {
                item.Value.AddLowPriced(flip);
            }

            if (minAccountTier >= AccountTier.PREMIUM_PLUS)
                await Task.Delay(900).ConfigureAwait(false);

            foreach (var item in Subs)
            {
                item.Value.AddLowPriced(flip);
            }

            StarterFlips.Enqueue(flip);

            if (flip.TargetPrice - flip.Auction.StartingBid < 250_000) // send below 200k profit out to everyone
            {
                foreach (var item in SlowSubs)
                {
                    item.Value.AddLowPriced(flip);
                }
            }
            else
                PrepareSlow(LowPriceToFlip(flip));
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
                    {
                        connection?.Connection?.Log("unsubed because of " + JsonConvert.SerializeObject(flip), Microsoft.Extensions.Logging.LogLevel.Error);
                        Unsubscribe(subscribers, item);
                    }
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
                if(SlowFlips.Count > 600)
                    return; // to large queue, continue immediately
                await Task.Delay(DelayTimeFor(SlowFlips.Count) ).ConfigureAwait(false);
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

            await Kafka.KafkaConsumer.ConsumeBatch<LowPricedAuction>(this.consumerConf.BootstrapServers, topics, flips =>
            {
                var time = (DateTime.UtcNow - flips.First().Auction.FindTime).TotalSeconds;
                runtroughTime.Observe(time);
                QueueLowPriced(flips.Where(flip => !(flip.Auction.Start.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime() - TimeSpan.FromMinutes(4)
                    && flip.Auction.Bin || flip.Auction.End < DateTime.UtcNow)));

                return Task.CompletedTask;
            }, CancellationToken.None, consumerConf.GroupId + Random.Shared.Next(), 50, AutoOffsetReset.Latest).ConfigureAwait(false);
        }

        public async Task ConsumeNewAuctions()
        {
            string[] topics = new string[] { AuctionConsumeTopic };

            await Kafka.KafkaConsumer.ConsumeBatch<SaveAuction>(this.consumerConf.BootstrapServers, topics, auctions =>
            {
                if (FilterSumary.UserFinderEnabledCount == 0)
                    return Task.CompletedTask;

                QueueLowPriced(auctions.Where(flip => !(flip.Start.ToUniversalTime() < DateTime.UtcNow.ToUniversalTime() - TimeSpan.FromMinutes(4)
                    && flip.Bin || flip.End < DateTime.UtcNow)).Select(auction => new LowPricedAuction()
                    {
                        Auction = auction,
                        DailyVolume = 0,
                        Finder = LowPricedAuction.FinderType.USER,
                        TargetPrice = auction.StartingBid
                    }));
                return Task.CompletedTask;
            }, CancellationToken.None, consumerConf.GroupId, 50, AutoOffsetReset.Latest).ConfigureAwait(false);
        }

        private void QueueLowPriced(IEnumerable<LowPricedAuction> flips)
        {
            Task.Run(async () =>
            {
                try
                {
                    await DeliverLowPricedAuctions(flips.ToList()).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    dev.Logger.Instance.Error(e, "delivering low priced auction");
                }
            }).ConfigureAwait(false);
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

        private async Task ConsumeBatch<T>(string[] topics, Action<T> work, int batchSize = 20)
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
                                await Task.Delay(10).ConfigureAwait(false);
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
                        if (batch.Count >= batchSize)
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

        public static int DelayTimeFor(int queueSize, double minutes = 3, int max = 10000)
        {
            return (int)Math.Min((TimeSpan.FromMinutes(minutes) / (Math.Max(queueSize, 1))).TotalMilliseconds, max);
        }

        /// <summary>
        /// Removes old <see cref="SoldAuctions"/>
        /// </summary>
        private void ClearSoldBuffer()
        {
            var toRemove = new List<long>();
            var oldestTime = DateTime.UtcNow - TimeSpan.FromMinutes(10);
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

        private void UpdateFilterSumaries()
        {
            var minProfit = long.MaxValue;
            var sumary = new ServerFilterSumary();
            foreach (var item in Connections)
            {
                var settings = item.Connection.Settings;
                if (settings == null)
                    continue;
                if (settings.AllowedFinders.HasFlag(LowPricedAuction.FinderType.USER))
                    sumary.UserFinderEnabledCount++;
                minProfit = Math.Min(minProfit, settings.MinProfit);
            }
            sumary.LowestMinProfit = minProfit;

            // assign after calculation so that there is no racecondition while calculating
            this.FilterSumary = sumary;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Instance = this;
            RunIsolatedForever(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (!StarterFlips.TryDequeue(out LowPricedAuction flip))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(13)).ConfigureAwait(false);
                        continue;
                    }
                    foreach (var item in StarterSubs.Keys)
                    {
                        if (!StarterSubs.TryGetValue(item, out FlipConWrapper con) || !con.AddLowPriced(flip) && con.Closed)
                        {
                            Console.WriteLine("removing starter premium user");
                            StarterSubs.TryRemove(item, out _);
                        }
                    }
                    Console.Write("sending starter flip ");
                    await Task.Delay(DelayTimeFor(StarterFlips.Count, 0.2, 2000)).ConfigureAwait(false);
                }
            }, "starter premium flips", stoppingToken);

            RunIsolatedForever(ListentoUnavailableTopics, "flip wait", stoppingToken);
            RunIsolatedForever(ListenToNewFlips, "flip wait", stoppingToken);
            RunIsolatedForever(ListenToLowPriced, "low priced auctions", stoppingToken);
            RunIsolatedForever(ConsumeNewAuctions, "consuming new auctions for user filter", stoppingToken);

            RunIsolatedForever(ProcessSlowQueue, "flip process slow", stoppingToken, 200);
            return Task.CompletedTask;
        }

        private static TaskFactory factory = new TaskFactory();
        public static void RunIsolatedForever(Func<Task> todo, string message, CancellationToken stoppingToken, int backoff = 2000)
        {
            factory.StartNew(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await todo().ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"{message}: {e.Message} {e.StackTrace}\n {e.InnerException?.Message} {e.InnerException?.StackTrace} {e.InnerException?.InnerException?.Message} {e.InnerException?.InnerException?.StackTrace}");
                        await Task.Delay(2000).ConfigureAwait(false);
                    }
                    await Task.Delay(backoff, stoppingToken).ConfigureAwait(false);
                }
            }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        public List<FlipConWrapper> Connections
        {
            get
            {
                var list = new List<FlipConWrapper>();
                list.AddRange(SuperSubs.Values);
                list.AddRange(Subs.Values);
                list.AddRange(SlowSubs.Values);
                return list;
            }
        }
    }
    public class ServerFilterSumary
    {
        public int UserFinderEnabledCount;
        public long LowestMinProfit;
    }
}
