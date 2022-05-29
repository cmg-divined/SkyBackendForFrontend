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
            Assert.AreEqual("Dragon Hunter: 5", prop);
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
            Assert.AreEqual("Bed: 6s", prop);
        }
    }
}