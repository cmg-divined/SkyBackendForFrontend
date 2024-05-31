
using System;
using System.Collections.Generic;
using NUnit.Framework;
using static Coflnet.Sky.Commands.Shared.CurrentEventDetailedFlipFilter;

namespace Coflnet.Sky.Commands.Shared;

public class CurrentEventTests
{
    private class StaticTimeFilter : CurrentEventDetailedFlipFilter
    {
        public DateTime Time { get; set; } = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        protected override DateTime Now => Time;
    }
    [Test]
    public void NewYear()
    {
        AssertEvent(new DateTime(2023, 6, 2, 12, 55, 0, DateTimeKind.Utc), Events.NewYear);
    }
    [Test]
    public void TravelingZooWinter()
    {
        AssertEvent(new DateTime(2023, 6, 3, 20, 55, 0, DateTimeKind.Utc), Events.TravelingZoo);
    }
    [Test]
    public void TravelingZooSummer()
    {
        AssertEvent(new DateTime(2023, 6, 1, 6, 55, 0, DateTimeKind.Utc), Events.TravelingZoo);
    }
    [Test]
    public void SpookyFestival()
    {
        AssertEvent(new DateTime(2023, 6, 5, 23, 35, 0, DateTimeKind.Utc), Events.SpookyFestival);
    }
    [Test]
    public void SeasonOfJerry()
    {
        AssertEvent(new DateTime(2023, 6, 2, 11, 15, 1, DateTimeKind.Utc), Events.SeasonOfJerry);
    }
    [Test]
    public void NotSeasonOfJerry()
    {
        AssertEvent(new DateTime(2023, 6, 3, 11, 15, 1, DateTimeKind.Utc), Events.NotSeasonOfJerry);
    }
    [Test]
    public void DarkAuction()
    {
        AssertEvent(new DateTime(2023, 6, 3, 11, 55, 1, DateTimeKind.Utc), Events.DarkAuction);
    }
    [Test]
    public void NotDarkAuction()
    {
        AssertEvent(new DateTime(2023, 6, 3, 11, 54, 1, DateTimeKind.Utc), Events.NotDarkAuction);
    }


    private static void AssertEvent(DateTime start, Events target, Events eventBefore = Events.None)
    {
        AssertMatch(start, target);
        AssertMatch(start + TimeSpan.FromHours(1) - TimeSpan.FromSeconds(1), target);
        AssertMatch(start - TimeSpan.FromSeconds(1), eventBefore);
    }

    private static void AssertMatch(DateTime time, Events value)
    {
        var filter = new StaticTimeFilter();
        filter.Time = time;
        var expression = filter.GetExpression(new Dictionary<string, string>(), value.ToString());
        var compiled = expression.Compile();
        var flip = new FlipInstance() { Auction = new() };
        Assert.That(compiled(flip), $"Expected {value} for {time}");
    }

}