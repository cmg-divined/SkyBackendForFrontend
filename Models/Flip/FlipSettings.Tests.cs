using NUnit.Framework;
using Coflnet.Sky.Core;
using System.Diagnostics;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipSettingsTests
    {
        private const string SwordTag = "ASPECT_OF_THE_END";
        private const string PetTag = "ASPECT_OF_THE_END";
        private FlipInstance flipA;

        private FlipInstance flipB;
        private FlipSettings settings;

        [SetUp]
        public void Setup()
        {
            flipA = new FlipInstance()
            {
                Auction = new SaveAuction()
                {
                    Enchantments = new System.Collections.Generic.List<Enchantment>()
                    {
                        new Enchantment(Enchantment.EnchantmentType.critical,6)
                    },
                    Tag = SwordTag
                },
                MedianPrice = 1000
            };

            flipB = new FlipInstance()
            {
                Auction = new SaveAuction()
                {
                    Enchantments = new System.Collections.Generic.List<Enchantment>()
                    {
                        new Enchantment(Enchantment.EnchantmentType.sharpness,6)
                    },
                    Tag = SwordTag
                },
                MedianPrice = 500
            };
            CreateLookup(flipA);
            CreateLookup(flipB);

            settings = new FlipSettings()
            {
                WhiteList = new System.Collections.Generic.List<ListEntry>()
                {
                    new ListEntry(){ItemTag = SwordTag, filter = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "Enchantment","critical"}, {"EnchantLvl", "6"}
                    }},
                    new ListEntry(){ItemTag = PetTag, filter = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "PetLevel","1-80"}, {"Rarity", "UNCOMMON"}
                    }}
                },
                BlackList = new System.Collections.Generic.List<ListEntry>()
                {
                    new ListEntry(){ItemTag = SwordTag, filter = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "Enchantment","sharpness"}, {"EnchantLvl", "6"}
                    }},
                    new ListEntry(){ItemTag = "A"},
                    new ListEntry(){ItemTag = "A"},
                    new ListEntry(){ItemTag = "b"},
                    new ListEntry(){ItemTag = "o"},
                    new ListEntry(){ItemTag = "i"},
                    new ListEntry(){ItemTag = "u"},
                    new ListEntry(){ItemTag = "Az"},
                    new ListEntry(){ItemTag = "Aq"},
                    new ListEntry(){ItemTag = "Aw"},
                    new ListEntry(){ItemTag = "Ae"},
                    new ListEntry(){ItemTag = "Ar"},
                    new ListEntry(){ItemTag = "At"},
                    new ListEntry(){ItemTag = "Aa"},
                    new ListEntry(){ItemTag = "As"},
                    new ListEntry(){ItemTag = "Ad"},
                    new ListEntry(){ItemTag = "Af"},
                    new ListEntry(){ItemTag = "Ag"},
                    new ListEntry(){ItemTag = "Ay"},
                    new ListEntry(){ItemTag = "Ax"},
                    new ListEntry(){ItemTag = "Ac"},
                    new ListEntry(){ItemTag = PetTag, filter = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "PetLevel","<90"}, {"Rarity", "COMMON"}
                    }}
                },
            };

            NBT.Instance = new MockNbt();
        }

        private static void CreateLookup(FlipInstance flip)
        {
            flip.Auction.NBTLookup = new System.Collections.Generic.List<NBTLookup>() { new NBTLookup(1, 2) };
        }

        [Test]
        public void MatchesSettings1()
        {

            Assert.IsTrue(settings.MatchesSettings(flipA).Item1);
            var bRes = settings.MatchesSettings(flipB);
            Assert.IsFalse(bRes.Item1, bRes.Item2);
            flipB.Auction.Enchantments.AddRange(flipA.Auction.Enchantments);
            bRes = settings.MatchesSettings(flipB);
            Assert.IsTrue(bRes.Item1, bRes.Item2);
        }
        [Test]
        public void BlacklitPetLevel()
        {
            flipB.Auction.Tag = PetTag;
            flipB.Auction.NBTLookup.Add(new NBTLookup(1, 2));
            var bRes = settings.MatchesSettings(flipB);
            Assert.IsFalse(bRes.Item1, bRes.Item2);
        }


        [Test]
        public void MatchesSpeed()
        {
            var matchCount = 0;
            var iterations = 2000;
            settings.MatchesSettings(flipB); // compile
            var stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                if (settings.MatchesSettings(flipA).Item1)
                    matchCount++;
            }
            Assert.Greater(60, stopWatch.ElapsedMilliseconds, "matching is too slow");
            stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                if (!settings.MatchesSettings(flipB).Item1)
                    matchCount++;
            }
            Assert.Greater(140, stopWatch.ElapsedMilliseconds, "matching blacklist is too slow");
            Assert.AreEqual(iterations * 2, matchCount);
        }


        [Test]
        public void CreateListMatcherWithNull()
        {
            var matcher = new FlipSettings.ListMatcher(null);
            Assert.IsFalse(matcher.IsMatch(flipA).Item1);
        }

        [Test]
        public void ReportFromartificialair()
        {
            var listEntry = new ListEntry()
            {
                ItemTag = "SORROW_HELMET",
                filter = new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "Reforge","Renowned"}, {"Enchantment","ultimate_legion"}, {"EnchantLvl","1-5"}, {"ProfitPercentage","1-22"}
                }
            };

            var auction = new FlipInstance()
            {
                MedianPrice = 32_000_000,
                Auction = new SaveAuction()
                {
                    Tag = "SORROW_HELMET",
                    Enchantments = new System.Collections.Generic.List<Enchantment>()
                    {
                        new Enchantment()
                        {
                            Type = Enchantment.EnchantmentType.ultimate_legion,
                            Level = 4
                        }
                    },
                    FlatenedNBT = new System.Collections.Generic.Dictionary<string, string>()
                    {
                        { "rarity_upgrades", "1" },
                        { "hpc", "15" },
                        { "unlocked_slots", "JADE_0" },
                        { "uid", "83de32552763" }
                    },
                    StartingBid = 28_000_000,
                    Reforge = ItemReferences.Reforge.Renowned
                }
            };

            var profitp = auction.ProfitPercentage;

            Assert.IsTrue(listEntry.MatchesSettings(auction));
        }
        [Test]
        public void MinProfitPercentageForHyperion()
        {
            var listEntry = new ListEntry()
            {
                ItemTag = "HYPERION",
                filter = new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "FlipFinder", "SNIPER_MEDIAN"}, {"MinProfitPercentage", "60"}
                }
            };

            var auction = new FlipInstance()
            {
                MedianPrice = 820000000,
                Finder = LowPricedAuction.FinderType.SNIPER_MEDIAN,
                Auction = new SaveAuction()
                {
                    Tag = "HYPERION",
                    StartingBid = 1000000,
                    Reforge = ItemReferences.Reforge.Heroic
                }
            };
            // report https://discord.com/channels/267680588666896385/986382602376196116/986382624362737685
            Assert.IsTrue(listEntry.MatchesSettings(auction));
        }
    }

    public class MockNbt : INBT
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
