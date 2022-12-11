using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.FlipTracker.Client.Api;
using Confluent.Kafka;
using Coflnet.Sky.Core;
using Microsoft.EntityFrameworkCore;
using Coflnet.Sky.Commands.Shared;
using Coflnet.Sky.FlipTracker.Client.Model;
using OpenTracing.Util;
using Newtonsoft.Json;
using OpenTracing;

namespace Coflnet.Sky.Commands
{
    public partial class FlipTrackingService
    {
        public TrackerApi flipTracking;
        private AnalyseApi flipAnalyse;

        //public static FlipTrackingService Instance = new FlipTrackingService();

        private static string ProduceTopic;
        private static ProducerConfig producerConfig;
        private GemPriceService gemPriceService;
        private UpgradePriceService priceService;
        private ITracer tracer;

        IProducer<string, FlipTracker.Client.Model.FlipEvent> producer;

        static FlipTrackingService()
        {
            producerConfig = new ProducerConfig { BootstrapServers = SimplerConfig.Config.Instance["KAFKA_HOST"], CancellationDelayMaxMs = 1000 };
            ProduceTopic = SimplerConfig.Config.Instance["TOPICS:FLIP_EVENT"];
        }

        public FlipTrackingService(GemPriceService gemPriceService, UpgradePriceService priceService, ITracer tracer)
        {
            producer = new ProducerBuilder<string, FlipTracker.Client.Model.FlipEvent>(new ProducerConfig
            {
                BootstrapServers = SimplerConfig.Config.Instance["KAFKA_HOST"],
                CancellationDelayMaxMs = 1000
            })
                    .SetValueSerializer(SerializerFactory.GetSerializer<FlipTracker.Client.Model.FlipEvent>()).Build();
            flipTracking = new TrackerApi("http://" + SimplerConfig.Config.Instance["FLIPTRACKER_HOST"]);
            flipAnalyse = new AnalyseApi("http://" + SimplerConfig.Config.Instance["FLIPTRACKER_HOST"]);
            this.gemPriceService = gemPriceService;
            this.priceService = priceService;
            this.tracer = tracer;
        }


        public async Task ReceiveFlip(string auctionId, string playerId, DateTime when = default)
        {
            try
            {
                await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.FLIPRECEIVE, when);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
                throw e;
            }
        }
        public async Task ClickFlip(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.FLIPCLICK);
        }
        public async Task PurchaseStart(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.PURCHASESTART);
        }
        public async Task PurchaseConfirm(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.PURCHASECONFIRM);
        }
        public async Task Sold(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.AUCTIONSOLD);
        }
        public async Task UpVote(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.UPVOTE);
        }
        public async Task DownVote(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.DOWNVOTE);
        }

        private Task SendEvent(string auctionId, string playerId, FlipTracker.Client.Model.FlipEventType type, DateTime when = default)
        {
            var flipEvent = new FlipTracker.Client.Model.FlipEvent()
            {
                Type = type,
                PlayerId = AuctionService.Instance.GetId(playerId),
                AuctionId = AuctionService.Instance.GetId(auctionId),
                Timestamp = when == default ? System.DateTime.UtcNow : when
            };

            producer.Produce(ProduceTopic, new Message<string, FlipTracker.Client.Model.FlipEvent>() { Value = flipEvent });
            return Task.CompletedTask;
        }

        public async Task NewFlip(LowPricedAuction flip, DateTime foundAt = default)
        {
            var res = await flipTracking.TrackerFlipAuctionIdPostAsync(flip.Auction.Uuid, new FlipTracker.Client.Model.Flip()
            {
                FinderType = (FlipTracker.Client.Model.FinderType?)flip.Finder,
                TargetPrice = (int)flip.TargetPrice,
                Timestamp = foundAt,
                AuctionId = flip.UId
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Recomended delay for a given player
        /// </summary>
        /// <param name="playerIds">The uuid of the player to test</param>
        /// <returns></returns>
        public async Task<(System.TimeSpan, int)> GetRecommendedPenalty(IEnumerable<string> playerIds)
        {
            var breakdown = await GetSpeedComp(playerIds);
            var hourCount = breakdown?.Times?.Where(t => t.TotalSeconds > 1).GroupBy(t => System.TimeSpan.Parse(t.Age).Hours).Count() ?? 0;
            return (System.TimeSpan.FromSeconds(breakdown?.Penalty ?? 0), hourCount);
        }

        /// <summary>
        /// Get the speed data for a given player
        /// </summary>
        public virtual async Task<SpeedCompResult> GetSpeedComp(IEnumerable<string> playerIds)
        {
            return await flipAnalyse.PlayersSpeedPostAsync(new SpeedCheckRequest(playerIds.ToList())).ConfigureAwait(false);
        }

        public async Task<int> ActiveFlipperCount()
        {
            return await flipAnalyse.UsersActiveCountGetAsync().ConfigureAwait(false);
        }

        public async Task<List<FlipDetails>> GetFlipsForFinder(LowPricedAuction.FinderType type, DateTime start, DateTime end)
        {
            if (start > end)
            {
                var tmp = end;
                end = start;
                start = tmp;
            }
            if (start < end - System.TimeSpan.FromDays(1))
                throw new CoflnetException("span_to_large", "Querying for more than a day is not supported");

            var idTask = flipAnalyse.AnalyseFinderFinderTypeGetAsync(Enum.Parse<FinderType>(type.ToString(), true), start, end).ConfigureAwait(false);
            using (var context = new HypixelContext())
            {
                var receivedFlips = await idTask;
                if (receivedFlips == null)
                    throw new CoflnetException("retrieve_failed", "Could not retrieve data from the flip tracker");
                var flips = receivedFlips.GroupBy(r => r.AuctionId).Select(g => g.First()).ToDictionary(f => f.AuctionId);
                var ids = flips.Keys;
                var buyList = await context.Auctions.Where(a => ids.Contains(a.UId) && a.HighestBidAmount > 0)
                    .Include(a => a.NBTLookup)
                    .AsNoTracking()
                    .ToListAsync().ConfigureAwait(false);
                // only include flips that were bought shortly after being reported
                //buyList = buyList.Where(a => !flips.TryGetValue(a.UId, out Flip f) || f.Timestamp < a.End && f.Timestamp > a.End - TimeSpan.FromSeconds(50)).ToList();

                GlobalTracer.Instance.ActiveSpan.Log(flips.Count.ToString());
                GlobalTracer.Instance.ActiveSpan.Log(JsonConvert.SerializeObject(flips.Take(5)));
                GlobalTracer.Instance.ActiveSpan.Log(buyList.Count.ToString());
                GlobalTracer.Instance.ActiveSpan.Log(JsonConvert.SerializeObject(buyList.Take(5)));
                var uidKey = NBT.Instance.GetKeyId("uid");
                var buyLookup = buyList
                    .Where(a => a.NBTLookup.Where(l => l.KeyId == uidKey).Any())
                    .GroupBy(a =>
                    {
                        return a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value;
                    }).ToDictionary(b => b.Key);
                var buyUidLookup = buyLookup.Select(a => a.Key).ToHashSet();
                var sellIds = await context.NBTLookups.Where(b => b.KeyId == uidKey && buyUidLookup.Contains(b.Value)).AsNoTracking().Select(n => n.AuctionId).ToListAsync();
                var buyAuctionUidLookup = buyLookup.Select(a => a.Value.First().UId).ToHashSet();
                var sells = await context.Auctions.Where(b => sellIds.Contains(b.Id) && !buyAuctionUidLookup.Contains(b.UId) && b.End > start && b.HighestBidAmount > 0 && b.End < DateTime.Now)
                                        .Select(s => new { s.End, s.HighestBidAmount, s.NBTLookup, s.Uuid }).AsNoTracking().ToListAsync().ConfigureAwait(false);

                return sells.Select(s =>
                {
                    var uid = s.NBTLookup.Where(b => b.KeyId == uidKey).FirstOrDefault().Value;
                    var buy = buyLookup.GetValueOrDefault(uid)?.OrderBy(b => b.End).Where(b => b.Uuid != s.Uuid).FirstOrDefault();
                    if (buy == null)
                        return null;
                    // make sure that this is the correct sell of this flip
                    if (buy.End > s.End)
                        return null;
                    if (buy.HighestBidAmount == 0)
                        return null;

                    var profit = gemPriceService.GetGemWrthFromLookup(buy.NBTLookup)
                                - gemPriceService.GetGemWrthFromLookup(s.NBTLookup)
                                + s.HighestBidAmount * 98 / 100
                                - buy.HighestBidAmount;


                    return new FlipDetails()
                    {
                        BuyTime = buy.End,
                        Finder = type,
                        ItemName = buy.ItemName,
                        ItemTag = buy.Tag,
                        OriginAuction = buy.Uuid,
                        PricePaid = buy.HighestBidAmount,
                        SellTime = s.End,
                        SoldAuction = s.Uuid,
                        SoldFor = s.HighestBidAmount,
                        Tier = buy.Tier.ToString(),
                        uId = uid,
                        Profit = profit
                    };
                }).Where(f => f != null).GroupBy(s => s.OriginAuction)
                .Select(s => s.Where(s => s.SellTime > s.BuyTime).OrderBy(s => s.SellTime).FirstOrDefault())
                .Where(f => f != null)
                .ToList();
            }

        }
        public async Task<FlipSumary> GetPlayerFlips(string uuid, System.TimeSpan timeSpan)
        {
            return await GetPlayerFlips(new string[] { uuid }, timeSpan);
        }

        public async Task<FlipSumary> GetPlayerFlips(IEnumerable<string> uuids, System.TimeSpan timeSpan)
        {
            using var context = new HypixelContext();

            var playerIds = await context.Players.Where(p => uuids.Contains(p.UuId)).AsNoTracking().Select(p => p.Id).ToListAsync();
            var startTime = DateTime.Now - timeSpan;
            var uidKey = NBT.Instance.GetKeyId("uid");
            var sellList = await context.Auctions.Where(a => playerIds.Contains(a.SellerId))
                .Where(a => a.End > startTime && a.End < DateTime.Now && a.HighestBidAmount > 0)
                .Include(a => a.NBTLookup)
                .Include(a => a.Enchantments)
                .AsNoTracking()
                .ToListAsync().ConfigureAwait(false);

            var sells = sellList
                .Where(a => a.NBTLookup.Where(l => l.KeyId == uidKey).Any())
                .GroupBy(a =>
                {
                    return a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value;
                }).ToList();
            var SalesUidLookup = sells.Select(a => a.Key).ToHashSet();
            var sales = await context.NBTLookups.Where(b => b.KeyId == uidKey && SalesUidLookup.Contains(b.Value)).AsNoTracking().Select(n => n.AuctionId).ToListAsync();
            var playerBids = await context.Bids.Where(b => playerIds.Contains(b.BidderId) && sales.Contains(b.Auction.Id) && b.Timestamp > startTime.Subtract(System.TimeSpan.FromDays(14)))
                .AsNoTracking()
                // filtering
                .OrderByDescending(bid => bid.Id)
                .Select(b => new
                {
                    b.Auction.Uuid,
                    b.Auction.End,
                    b.Auction.Tag,
                    b.Auction.Tier,
                    b.Amount,
                    Enchants = b.Auction.Enchantments,
                    Nbt = b.Auction.NBTLookup
                }).GroupBy(b => b.Uuid)
                .Select(bid => new BidQuery(
                    bid.Key,
                    bid.Max(b => b.Amount),
                    bid.Max(b => b.Amount),
                    bid.Max(b => b.End),
                    bid.First().Tag,
                    bid.First().Tier,
                    bid.OrderByDescending(b => b.Amount).First().Nbt,
                    bid.First().Enchants
                ))
                //.ThenInclude (b => b.Auction)
                .ToListAsync().ConfigureAwait(false);

            var flipStats = (await flipTracking.TrackerBatchFlipsPostAsync(playerBids.Select(b => AuctionService.Instance.GetId(b.Key)).ToList()))
                    ?.GroupBy(t => t.AuctionId)
                    ?.ToDictionary(t => t.Key, v => v.AsEnumerable());
            var flips = playerBids.Where(a => SalesUidLookup.Contains(a.Nbt.Where(b => b.KeyId == uidKey).FirstOrDefault().Value)).Select(b =>
            {

                return ToFlipDetails(b, uidKey, sells, flipStats);

            }).OrderByDescending(f => f.Profit).ToArray();

            return new FlipSumary()
            {
                Flips = flips,
                TotalProfit = flips.Sum(r => r.Profit)
            };

        }

        private FlipDetails ToFlipDetails(BidQuery b, short uidKey, List<IGrouping<long, SaveAuction>> sells, Dictionary<long, IEnumerable<Flip>> flipStats)
        {
            FlipTracker.Client.Model.Flip first = flipStats?.GetValueOrDefault(AuctionService.Instance.GetId(b.Key))?.OrderBy(b => b.Timestamp).FirstOrDefault();
            var uId = b.Nbt.Where(b => b.KeyId == uidKey).FirstOrDefault().Value;
            var sell = sells.Where(s => s.Key == uId)?
                    .FirstOrDefault()
                    ?.OrderByDescending(b => b.End)
                    .FirstOrDefault();
            try
            {
                return ToFlipDetails(b, first, uId, sell);
            }
            catch
            {
                tracer.ActiveSpan.Log(JsonConvert.SerializeObject(b));
                tracer.ActiveSpan.Log(JsonConvert.SerializeObject(sell));
                throw;
            }
        }

        private FlipDetails ToFlipDetails(BidQuery b, Flip first, long uId, SaveAuction sell)
        {
            var soldFor = sell
                                ?.HighestBidAmount;

            var enchantsBad = b.Tag == "ENCHANTED_BOOK" && (b.Enchants.Count == 1 && sell.Enchantments.Count != 1 || b.Enchants.First().Level != sell.Enchantments.First().Level)
                                && (sell.HighestBidAmount - b.HighestOwnBid) > 1_000_000;
            var profit = 1L;
            var changeSumary = new List<PropertyChange>();
            if (b.Tag == sell.Tag
                && !enchantsBad)
            {
                var gemSumaryBuy = gemPriceService.LookupToGems(b.Nbt);
                var gemSumarySell = gemPriceService.LookupToGems(sell.NBTLookup);

                changeSumary.AddRange(gemSumaryBuy);
                changeSumary.AddRange(gemSumarySell.Select(g => new PropertyChange()
                {
                    Description = $"Selling with {g.Description}",
                    Effect = -g.Effect
                }));
                try
                {
                    changeSumary.AddRange(GetChanges(b, sell));
                } catch(Exception e)
                {
                    tracer.ActiveSpan.Log(e.ToString());
                    changeSumary.Add(new("Error occured " + tracer.ActiveSpan.Context.TraceId, -1));
                }
                var tax = sell.HighestBidAmount - sell.HighestBidAmount * 98 / 100;
                changeSumary.Add(new PropertyChange()
                {
                    Description = $"2% AH tax for sell",
                    Effect = -tax
                });

                profit = changeSumary.Sum(g => g.Effect)
                + sell.HighestBidAmount
                - b.HighestOwnBid;
            }


            return new FlipDetails()
            {
                Finder = (first == null ? LowPricedAuction.FinderType.UNKOWN : Enum.Parse<LowPricedAuction.FinderType>(
                    first.FinderType.ToString().Replace("SNIPERMEDIAN", "SNIPER_MEDIAN"), true)),
                OriginAuction = b.Key,
                ItemTag = sell.Tag,
                Tier = sell.Tier.ToString(),
                SoldAuction = sell?.Uuid,
                PricePaid = b.HighestOwnBid,
                SoldFor = soldFor ?? 0,
                uId = uId,
                ItemName = sell?.ItemName,
                BuyTime = b.End,
                SellTime = sell.End,
                Profit = profit,
                PropertyChanges = changeSumary
            };
        }

        private IEnumerable<PropertyChange> GetChanges(BidQuery b, SaveAuction sell)
        {
            if (b == null || sell.Tag == null)
                yield break;
            if (b.Tier < sell.Tier)
                if (sell.Tag.StartsWith("PET_"))
                {
                    if (sell.NBTLookup.Where(l => l.KeyId == NBT.Instance.GetKeyId("heldItem") && l.Value == ItemDetails.Instance.GetItemIdForTag("TIER_BOOST")).Any())
                        yield return new("Tier Boost cost", priceService.GetTierBoostCost());
                    else
                    {
                        var cost = priceService.GetKatPrice(sell.Tag, sell.Tier);
                        yield return new("Kat upgrade", (long)-cost.UpgradeCost);
                        if (cost.MaterialCost > 0)
                            yield return new("Kat materials", (long)-cost.MaterialCost);
                    }
                }
                else
                    yield return new("Recombobulator", (long)-priceService.GetPrice("RECOMBOBULATOR_3000"));
        }
    }
}