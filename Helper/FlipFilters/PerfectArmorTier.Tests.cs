using NUnit.Framework;

namespace Coflnet.Sky.Commands.Shared;

public class PerfectArmorTierTests
{
    [TestCase("PERFECT_HELMET_1", "1", true)]
    [TestCase("PERFECT_HELMET_1", "1-10", true)]
    [TestCase("PERFECT_HELMET_1", ">2", false)]
    [TestCase("PERFECT_HELMET_11", ">2", true)]
    public void Match(string tag, string selector, bool expected)
    {
        var filter = new PerfectArmorTierDetailedFlipFilter();
        var flip = new FlipInstance()
        {
            Tag = tag,
        };
        Assert.AreEqual(expected, filter.GetExpression(null, selector).Compile()(flip));
    }
}