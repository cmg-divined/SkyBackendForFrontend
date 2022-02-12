using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coflnet.Sky.Commands.Shared;
using Coflnet.Sky.Filter;
using hypixel;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Coflnet.Sky.Commands.Tests
{
    public class FlipperServiceTests
    {
        [Test]
        public async Task ReceiveAndDistribute()
        {
            var service = new FlipperService();
            var con = new MockConnection();
            service.AddConnection(con);
            for (int i = 0; i < 20; i++)
                service.AddConnection(new MockConnection());
            var auction = new SaveAuction() { NbtData = new NbtData(), Enchantments = new System.Collections.Generic.List<Enchantment>() };
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                await service.DeliverLowPricedAuction(new LowPricedAuction()
                {
                    Auction = auction,
                    DailyVolume = 2,
                    Finder = LowPricedAuction.FinderType.AI,
                    TargetPrice = 5
                });
            }
            await Task.Delay(10); // wait for the async sending to finish
            Assert.Less(watch.ElapsedMilliseconds, 40);
            Assert.AreEqual(5, con.LastFlip.MedianPrice);
            Assert.AreEqual(2, con.LastFlip.Volume);
            Assert.AreEqual(auction, con.LastFlip.Auction);
        }

        public class MockConnection : IFlipConnection
        {
            public FlipSettings Settings => new FlipSettings();

            public long Id => new Random().NextInt64();

            public int UserId => 1;

            public SettingsChange LatestSettings => new SettingsChange()
            {
                Tier = AccountTier.PREMIUM,
                ExpiresAt = DateTime.Now + TimeSpan.FromHours(2)
            };

            public FlipInstance LastFlip;

            public Task<bool> SendFlip(FlipInstance flip)
            {
                LastFlip = flip;
                return Task.FromResult(true);
            }

            public Task<bool> SendFlip(LowPricedAuction flip)
            {
                return this.SendFlip(FlipperService.LowPriceToFlip(flip));
            }

            public Task<bool> SendSold(string uuid)
            {
                throw new System.NotImplementedException();
            }

            public void UpdateSettings(SettingsChange settings)
            {
                throw new System.NotImplementedException();
            }

            public void Log(string message, LogLevel level = LogLevel.Information)
            {
                throw new NotImplementedException();
            }
        }
    }
}
