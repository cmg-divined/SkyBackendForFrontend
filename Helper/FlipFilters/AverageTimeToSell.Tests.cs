using NUnit.Framework;

namespace Coflnet.Sky.Commands.Shared;

public class AverageTimeToSellTests
{
    [TestCase("1d", 1, true)]
    [TestCase("<1d", 2, true)]
    [TestCase("<2h", 13, true)]
    [TestCase(">2h", 13, false)]
    [TestCase(">2d", 2, false)]
    [TestCase(">2w", 0.1f, false)]
    [TestCase(">1w", 0.1f, true)]
    public void MatchesCases(string input, float volume, bool expected)
    {
        var filter = new AverageTimeToSellDetailedFlipFilter();
        Assert.That(filter.GetExpression(null, input).Compile()(new FlipInstance() { Volume = volume }), Is.EqualTo(expected));
    }
}