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
            flip.Auction.NBTLookup = new System.Collections.Generic.List<NBTLookup>(){new NBTLookup(1,2)};
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
            flipB.Auction.NBTLookup.Add(new NBTLookup(1,2));
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
                if(settings.MatchesSettings(flipA).Item1)
                    matchCount++;
            }
            Assert.Greater(10, stopWatch.ElapsedMilliseconds, "matching is too slow");
            stopWatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                if(!settings.MatchesSettings(flipB).Item1)
                    matchCount++;
            }
            Assert.Greater(17, stopWatch.ElapsedMilliseconds, "matching blacklist is too slow");
            Assert.AreEqual(iterations* 2,matchCount );
        }


        [Test]
        public void CreateListMatcherWithNull()
        {
            var matcher = new FlipSettings.ListMatcher(null);
            Assert.IsFalse(matcher.IsMatch(flipA).Item1);
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
