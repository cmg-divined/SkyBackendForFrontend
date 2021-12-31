using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.FlipTracker.Client.Api;
using Confluent.Kafka;
using hypixel;
using Microsoft.EntityFrameworkCore;
using Coflnet.Sky.Commands.Shared;

namespace Coflnet.Sky.Commands
{
    public partial class FlipTrackingService
    {
        public TrackerApi flipTracking;
        private AnalyseApi flipAnalyse;

        public static FlipTrackingService Instance = new FlipTrackingService();

        private static string ProduceTopic;
        private static ProducerConfig producerConfig;

        IProducer<string, FlipTracker.Client.Model.FlipEvent> producer;

        static FlipTrackingService()
        {
            producerConfig = new ProducerConfig { BootstrapServers = SimplerConfig.Config.Instance["KAFKA_HOST"], CancellationDelayMaxMs = 1000 };
            ProduceTopic = SimplerConfig.Config.Instance["TOPICS:FLIP_EVENT"];
        }

        public FlipTrackingService()
        {
            producer = new ProducerBuilder<string, FlipTracker.Client.Model.FlipEvent>(new ProducerConfig
            {
                BootstrapServers = SimplerConfig.Config.Instance["KAFKA_HOST"],
                CancellationDelayMaxMs = 1000
            })
                    .SetValueSerializer(hypixel.SerializerFactory.GetSerializer<FlipTracker.Client.Model.FlipEvent>()).Build();
            flipTracking = new TrackerApi("http://" + SimplerConfig.Config.Instance["FLIPTRACKER_HOST"]);
            flipAnalyse = new AnalyseApi("http://" + SimplerConfig.Config.Instance["FLIPTRACKER_HOST"]);
        }


        public async Task ReceiveFlip(string auctionId, string playerId)
        {
            try
            {
                await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_1);
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
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_2);
        }
        public async Task PurchaseStart(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_4);
        }
        public async Task PurchaseConfirm(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_8);
        }
        public async Task Sold(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_16);
        }
        public async Task UpVote(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_32);
        }
        public async Task DownVote(string auctionId, string playerId)
        {
            await SendEvent(auctionId, playerId, FlipTracker.Client.Model.FlipEventType.NUMBER_64);
        }

        private Task SendEvent(string auctionId, string playerId, FlipTracker.Client.Model.FlipEventType type)
        {
            var flipEvent = new FlipTracker.Client.Model.FlipEvent()
            {
                Type = type,
                PlayerId = hypixel.AuctionService.Instance.GetId(playerId),
                AuctionId = hypixel.AuctionService.Instance.GetId(auctionId),
                Timestamp = System.DateTime.Now
            };

            producer.Produce(ProduceTopic, new Message<string, FlipTracker.Client.Model.FlipEvent>() { Value = flipEvent });
            return Task.CompletedTask;
        }

        public async Task NewFlip(LowPricedAuction flip)
        {
            var res = await flipTracking.TrackerFlipAuctionIdPostAsync(flip.Auction.Uuid, new FlipTracker.Client.Model.Flip()
            {
                FinderType = (FlipTracker.Client.Model.FinderType?)flip.Finder,
                TargetPrice = flip.TargetPrice
            });
        }

        public async Task<List<FlipDetails>> GetFlipsForFinder(LowPricedAuction.FinderType type, DateTime start, DateTime end)
        {
            if (start > end)
            {
                var tmp = end;
                end = start;
                start = tmp;
            }
            if (start < end - TimeSpan.FromDays(1))
                throw new CoflnetException("span_to_large", "Querying for more than a day is not supported");

            var idTask = flipAnalyse.AnalyseFinderFinderTypeIdsGetAsync((Coflnet.Sky.FlipTracker.Client.Model.FinderType)type, start, end);
            using (var context = new HypixelContext())
            {
                var ids = await idTask;
                var buyList = await context.Auctions.Where(a => ids.Contains(a.UId))
                    .Include(a => a.NBTLookup)
                    .ToListAsync();
                
                var uidKey = NBT.Instance.GetKeyId("uid");
                var buyLookup = buyList
                    .Where(a => a.NBTLookup.Where(l => l.KeyId == uidKey).Any())
                    .GroupBy(a =>
                    {
                        return a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value;
                    }).ToDictionary(b=>b.Key);
                var buyUidLookup = buyLookup.Select(a => a.Key).ToHashSet();
                var sellIds = await context.NBTLookups.Where(b => b.KeyId == uidKey && buyUidLookup.Contains(b.Value)).Select(n => n.AuctionId).ToListAsync();
                var sells = await context.Auctions.Where(b =>sellIds.Contains(b.Id) && b.End > start).Select(s => new {s.End,s.HighestBidAmount,s.NBTLookup,s.Uuid}).ToListAsync();

                return sells.Select(s=>{
                    var uid = s.NBTLookup.Where(b => b.KeyId == uidKey).FirstOrDefault().Value;
                    var buy = buyLookup.GetValueOrDefault(uid)?.FirstOrDefault();
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
                        uId = uid
                    };
                }).ToList();
            }

        }

        public async Task<FlipSumary> GetPlayerFlips(string uuid, TimeSpan timeSpan)
        {
            using (var context = new HypixelContext())
            {
                var playerId = await context.Players.Where(p => p.UuId == uuid).Select(p => p.Id).FirstOrDefaultAsync();
                var startTime = DateTime.Now - timeSpan;
                var uidKey = NBT.Instance.GetKeyId("uid");
                var sellList = await context.Auctions.Where(a => a.SellerId == playerId)
                    .Where(a => a.End > startTime && a.End < DateTime.Now && a.HighestBidAmount > 0 && a.Bin)
                    .Include(a => a.NBTLookup)
                    .ToListAsync();

                var sells = sellList
                    .Where(a => a.NBTLookup.Where(l => l.KeyId == uidKey).Any())
                    .GroupBy(a =>
                    {
                        return a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value;
                    }).ToList();
                var SalesUidLookup = sells.Select(a => a.Key).ToHashSet();
                var sales = await context.NBTLookups.Where(b => b.KeyId == uidKey && SalesUidLookup.Contains(b.Value)).Select(n => n.AuctionId).ToListAsync();
                var playerBids = await context.Bids.Where(b => b.BidderId == playerId && sales.Contains(b.Auction.Id) && b.Timestamp > startTime.Subtract(TimeSpan.FromDays(14)))
                    //.Where(b => b.Auction.NBTLookup.Where(b => b.KeyId == uidKey && SalesUidLookup.Contains(b.Value)).Any())
                    // filtering
                    .OrderByDescending(bid => bid.Id)
                    .Select(b => new
                    {
                        b.Auction.Uuid,
                        b.Auction.HighestBidAmount,
                        b.Auction.End,
                        b.Amount,
                        itemUid = b.Auction.NBTLookup.Where(b => b.KeyId == uidKey).FirstOrDefault().Value
                    }).GroupBy(b => b.Uuid)
                    .Select(bid => new
                    {
                        bid.Key,
                        Amount = bid.Max(b => b.Amount),
                        HighestBid = bid.Max(b => b.HighestBidAmount),
                        HighestOwnBid = bid.Max(b => b.Amount),
                        End = bid.Max(b => b.End),
                        itemUid = bid.Max(b => b.itemUid)
                    })
                    //.ThenInclude (b => b.Auction)
                    .ToListAsync();

                var flipStats = (await flipTracking.TrackerBatchFlipsPostAsync(playerBids.Select(b => AuctionService.Instance.GetId(b.Key)).ToList()))
                        ?.GroupBy(t => t.AuctionId)
                        ?.ToDictionary(t => t.Key, v => v.AsEnumerable());
                var flips = playerBids.Where(a => SalesUidLookup.Contains(a.itemUid)).Select(b =>
                {
                    FlipTracker.Client.Model.Flip first = flipStats?.GetValueOrDefault(AuctionService.Instance.GetId(b.Key))?.OrderBy(b => b.Timestamp).FirstOrDefault();

                    var sell = sells.Where(s => s.Key == b.itemUid)?
                            .FirstOrDefault()
                            ?.OrderByDescending(b => b.End)
                            .FirstOrDefault();
                    var soldFor = sell
                            ?.HighestBidAmount;
                    return new FlipDetails()
                    {
                        Finder = (first == null ? LowPricedAuction.FinderType.UNKOWN : (LowPricedAuction.FinderType)first.FinderType),
                        OriginAuction = b.Key,
                        ItemTag = sell.Tag,
                        Tier = sell.Tier.ToString(),
                        SoldAuction = sell?.Uuid,
                        PricePaid = b.HighestOwnBid,
                        SoldFor = soldFor ?? 0,
                        uId = b.itemUid,
                        ItemName = sell?.ItemName,
                        BuyTime = b.End,
                        SellTime = sell.End
                    };
                }).ToArray();

                return new FlipSumary()
                {
                    Flips = flips,
                    TotalProfit = flips.Sum(r => r.SoldFor - r.PricePaid)
                };
            }
        }
    }
}