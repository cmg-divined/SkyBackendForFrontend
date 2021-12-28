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

        public async Task<FlipSumary> GetPlayerFlips(string uuid, TimeSpan timeSpan)
        {
            using (var context = new HypixelContext())
            {
                var playerId = await context.Players.Where(p => p.UuId == uuid).Select(p => p.Id).FirstOrDefaultAsync();
                var uidKey = NBT.Instance.GetKeyId("uid");
                var sellList = await context.Auctions.Where(a => a.SellerId == playerId)
                    .Where(a => a.End > DateTime.Now - timeSpan && a.HighestBidAmount > 0)
                    .Include(a => a.NBTLookup)
                    .Where(a => a.NBTLookup.Where(l => l.KeyId == uidKey).Any())
                    .ToListAsync();
                
                var sells = sellList
                    .GroupBy(a =>{
                        Console.WriteLine($"{a.ItemName} {a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value} {a.Uuid} {a.End}");
                        return a.NBTLookup.Where(l => l.KeyId == uidKey).FirstOrDefault().Value;}).ToList();
                var SalesUidLookup = sells.Select(a => a.Key).ToHashSet();
                var playerBids = await context.Bids.Where(b => b.BidderId == playerId).Where(b => b.Auction.NBTLookup.Where(b => b.KeyId == uidKey && SalesUidLookup.Contains(b.Value)).Any())
                    // filtering
                    .OrderByDescending(auction => auction.Id)
                    //.Include (p => p.Auction)
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
                        itemUid = bid.Max(b=>b.itemUid)
                    })
                    //.ThenInclude (b => b.Auction)
                    .ToListAsync();

                var flips = playerBids.Where(a =>SalesUidLookup.Contains(a.itemUid) ).Select(async b =>
                {
                    var flipStats = await flipTracking.TrackerFlipsAuctionIdGetAsync(AuctionService.Instance.GetId(b.Key));
                    var first = flipStats?.OrderBy(f=>f.Timestamp).FirstOrDefault();
                    var soldFor = sells.Where(s=>s.Key == b.itemUid)?
                            .FirstOrDefault()
                            ?.OrderByDescending(b=>b.End)
                            .FirstOrDefault()
                            ?.HighestBidAmount;
                    return new FlipDetails()
                    {
                        Finder = (first == null ? LowPricedAuction.FinderType.UNKOWN : (LowPricedAuction.FinderType)first.FinderType),
                        OriginAuction = b.Key,
                        PricePaid = b.HighestOwnBid,
                        SoldFor = soldFor ?? 0,
                        uId = b.itemUid
                    };
                });

                return new FlipSumary(){Flips = await Task.WhenAll(flips)};
            }
        }
    }
}