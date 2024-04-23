using System;
using System.Linq;
using Coflnet.Sky.Core;
using NUnit.Framework;

namespace Coflnet.Sky.Commands.Helper
{
    public class PropertiesSelectorTests
    {
        [Test]
        public void DragonHunter()
        {
            var auction = new SaveAuction()
            {
                Enchantments = new System.Collections.Generic.List<Enchantment>() {
                    new Enchantment(Enchantment.EnchantmentType.dragon_hunter, 5)
                    }
            };
            var prop = PropertiesSelector.GetProperties(auction).Select(p => p.Value).First();
            Assert.That("Dragon Hunter: 5",Is.EqualTo(prop));
        }
        [Test]
        public void BedTime()
        {
            var auction = new SaveAuction()
            {
                Enchantments = new System.Collections.Generic.List<Enchantment>(),
                Start = DateTime.UtcNow.AddSeconds(-10)
            };
            var prop = PropertiesSelector.GetProperties(auction).Select(p => p.Value).First();
            Assert.That("Bed: 9s",Is.EqualTo(prop));
        }
        [TestCase("0:0:255", "§10000FF")]
        [TestCase("0:190:0", "§200BE00")]
        [TestCase("10:0:17", "§00A0011")]
        public void FormatHex(string input, string shwon)
        {
            var auction = new SaveAuction()
            {
                FlatenedNBT = new System.Collections.Generic.Dictionary<string, string>()
                { { "color", input } }
            };
            var prop = PropertiesSelector.GetProperties(auction).Select(p => p.Value).First();
            Assert.That($"Color: {shwon}§f",Is.EqualTo(prop));
        }
    }
}