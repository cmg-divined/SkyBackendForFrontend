
using System.Collections.Generic;
using System.Linq;
using hypixel;
using NUnit.Framework;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFilterTests
    {
        FlipInstance sampleFlip = new FlipInstance()
        {
            MedianPrice = 10,
            Volume = 10,
            Auction = new SaveAuction()
            {
                Bin = false,
                Enchantments = new List<Enchantment>(){
                    new Enchantment(Enchantment.EnchantmentType.critical,4)
                },
                FlatenedNBT = new Dictionary<string, string>() { { "candy", "3" } }
            }
        };
        [Test]
        public void IsMatch()
        {
            var settings = new FlipSettings()
            {
                BlackList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() { { "Bin", "true" } } } }
            };
            var matches = settings.MatchesSettings(sampleFlip);
            Assert.IsTrue(matches.Item1, "flip should match");
            sampleFlip.Auction.Bin = true;
            Assert.IsFalse(settings.MatchesSettings(sampleFlip).Item1, "flip should not match");
        }


        [Test]
        public void EnchantmentMatch()
        {
            var settings = new FlipSettings()
            {
                BlackList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() { { "Enchantment", "aiming" }, { "EnchantLvl", "1" } } } }
            };
            var matches = settings.MatchesSettings(sampleFlip);
            Assert.IsTrue(matches.Item1, "flip should match");
        }


        [Test]
        public void EnchantmentBlacklistMatch()
        {
            var settings = new FlipSettings()
            {
                BlackList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() { { "Enchantment", "critical" }, { "EnchantLvl", "4" } } } }
            };
            var matches = settings.MatchesSettings(sampleFlip);
            Assert.IsFalse(matches.Item1, "flip should not match");
        }

        [Test]
        public void CandyBlacklistMatch()
        {
            NBT.Instance = new NBTMock();
            sampleFlip.Auction.NBTLookup = new List<NBTLookup>() { new NBTLookup(1, 2) };
            var settings = new FlipSettings()
            {
                BlackList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() { { "Candy", "any" } } } }
            };
            var matches = settings.MatchesSettings(sampleFlip);
            Assert.IsFalse(matches.Item1, "flip should not match");
        }

        [Test]
        public void WhitelistBookEnchantBlackistItem()
        {
            NBT.Instance = new NBTMock();
            var tag = "ENCHANTED_BOOK";
            FlipInstance bookOfa = CreatOfaAuction(tag);
            FlipInstance reaperOfa = CreatOfaAuction("REAPER");
            var oneForAllFilter = new Dictionary<string, string>() { { "Enchantment", "ultimate_one_for_all" }, { "EnchantLvl", "1" } };
            var settings = new FlipSettings()
            {
                BlackList = new List<ListEntry>() { new ListEntry() { ItemTag = "REAPER", filter = oneForAllFilter } },
                WhiteList = new List<ListEntry>() { new ListEntry() { ItemTag = "ENCHANTED_BOOK", filter = oneForAllFilter } }
            };
            var matches = settings.MatchesSettings(bookOfa);
            var shouldNotBatch = settings.MatchesSettings(reaperOfa);
            Assert.True(matches.Item1, "flip should match");
            Assert.IsFalse(shouldNotBatch.Item1, "flip should not match");
        }


        [Test]
        public void MinProfitFilterMatch()
        {
            NBT.Instance = new NBTMock();
            sampleFlip.Auction.NBTLookup = new List<NBTLookup>() { new NBTLookup(1, 2) };
            var settings = new FlipSettings()
            {
                MinProfit = 10000,
                WhiteList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() { { "MinProfit", "5" } } } }
            };
            var matches = settings.MatchesSettings(sampleFlip);
            System.Console.WriteLine(sampleFlip.Profit);
            Assert.IsTrue(matches.Item1, matches.Item2);
        }


        [Test]
        public void FlipFilterFinderCustomMinProfitNoBinMatch()
        {
            var settings = new FlipSettings()
            {
                MinProfit = 10000,
                WhiteList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() {
                    { "MinProfit", "5" },{"FlipFinder", "SNIPER_MEDIAN"},{"Bin","false"} } } }
            };
            sampleFlip.LastKnownCost = 10;
            sampleFlip.MedianPrice = 100;
            sampleFlip.Finder = LowPricedAuction.FinderType.SNIPER_MEDIAN;
            Matches(settings, sampleFlip);
            sampleFlip.Finder = LowPricedAuction.FinderType.FLIPPER;
            NoMatch(settings, sampleFlip);
        }
        [Test]
        public void FlipFilterFinderCustomMinProfitMatch()
        {
            var settings = new FlipSettings()
            {
                MinProfit = 10000,
                WhiteList = new List<ListEntry>() { new ListEntry() { filter = new Dictionary<string, string>() {
                    { "MinProfit", "5" } } } }
            };
            sampleFlip.LastKnownCost = 10;
            sampleFlip.MedianPrice = 100;
            sampleFlip.Finder = LowPricedAuction.FinderType.SNIPER_MEDIAN;
            Matches(settings, sampleFlip);
        }

        private static void Matches(FlipSettings targetSettings, FlipInstance flip)
        {
            var matches = targetSettings.MatchesSettings(flip);
            Assert.IsTrue(matches.Item1, matches.Item2);
        }
        private static void NoMatch(FlipSettings targetSettings, FlipInstance flip)
        {
            var matches = targetSettings.MatchesSettings(flip);
            Assert.IsFalse(matches.Item1, matches.Item2);
        }

        private static ListEntry CreateFilter(string key, string value)
        {
            return new ListEntry() { filter = new Dictionary<string, string>() { { key, value } } };
        }

        private static FlipInstance CreatOfaAuction(string tag)
        {
            return new FlipInstance()
            {
                MedianPrice = 10,
                Volume = 10,
                Auction = new SaveAuction()
                {
                    Tag = tag,
                    Enchantments = new List<Enchantment>(){
                    new Enchantment(Enchantment.EnchantmentType.ultimate_one_for_all,1)
                }
                }

            };
        }

        class NBTMock : INBT
        {
            public short GetKeyId(string name)
            {
                return 1;
            }

            public int GetValueId(short key, string value)
            {
                return 2;
            }
        }
    }
}