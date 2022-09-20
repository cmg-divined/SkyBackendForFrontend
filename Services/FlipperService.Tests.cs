using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coflnet.Sky.Commands.Shared;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Collections.Generic;

namespace Coflnet.Sky.Commands.Tests
{
    public static class TestConstants
    {
        public static int DelayMultiplier = 20;
    }
    public class FlipperServiceTests
    {
        // test disabled because it fails in kaniko [Test]
        public async Task ReceiveAndDistribute()
        {
            var service = new FlipperService();
            var con = new MockConnection();
            service.AddConnection(con);
            //for (int i = 0; i < 1; i++)
            //    service.AddConnection(new MockConnection());
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
                }).ConfigureAwait(false);
            }
            await Task.Delay(20 * TestConstants.DelayMultiplier).ConfigureAwait(false); // wait for the async sending to finish
            Assert.NotNull(con.LastFlip, "No flip was sent but should have been after " + watch.ElapsedMilliseconds + "ms");
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

            public AccountInfo AccountInfo => throw new NotImplementedException();

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

            public Task<bool> SendBatch(IEnumerable<LowPricedAuction> flips)
            {
                throw new NotImplementedException();
            }

            Task IFlipConnection.SendBatch(IEnumerable<LowPricedAuction> flips)
            {
                throw new NotImplementedException();
            }
        }
    }
}
