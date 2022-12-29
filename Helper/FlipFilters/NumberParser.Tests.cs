using NUnit.Framework;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class NumberParserTests
    {
        [Test]
        [TestCase("5m", 5_000_000)]
        [TestCase("5.2m", 5_200_000)]
        [TestCase("1.2b", 1_200_000_000)]
        [TestCase("1.2k", 1_200)]
        [TestCase("1,2k", 1_200)]
        [TestCase("12", 12)]
        [TestCase("0.1", 0.1)]
        [TestCase("0.1dxy", 0.1)]
        public void ConvertInputs(string val, double target)
        {
            Assert.AreEqual(target, NumberParser.Double(val), 0.0001);
        }
        [TestCase("1", 1)]
        [TestCase("1.9m", 1_900_000)]
        [TestCase("1k", 1_000)]
        public void ConvertInputsLong(string val, long target)
        {
            Assert.AreEqual(target, NumberParser.Long(val));
        }
    }
}