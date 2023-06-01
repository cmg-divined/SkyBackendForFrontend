
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

public class CurrentEventDetailedFlipFilter : DetailedFlipFilter
{
    public enum Events
    {
        None,
        TravelingZoo,
        SpookyFestival,
        DarkAuction,
        NewYear,
        SeasonOfJerry
    }
    public enum Months
    {
        EarlySpring,
        Spring,
        LateSpring,
        EarlySummer,
        Summer,
        LateSummer,
        EarlyAutumn,
        Autumn,
        LateAutumn,
        EarlyWinter,
        Winter,
        LateWinter
    }
    public object[] Options => Enum.GetValues(typeof(Events)).Cast<object>().ToArray();

    public FilterType FilterType => FilterType.Equal;
    protected virtual DateTime Now => DateTime.UtcNow;

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        var eventVal = (Events)Enum.Parse(typeof(Events), val);
        var currentDay = GetCurrentDay();
        var eventList = new List<(int, int, Events)>()
        {
            (12*31-3,12*31, Events.NewYear),
            (03*31,03*31+3, Events.TravelingZoo),
            (9*31,9*31+3, Events.TravelingZoo),
            ((int)Months.Autumn*31+28,(int)Months.LateAutumn*31, Events.SpookyFestival),
            (11*31+23,11*31+26, Events.SeasonOfJerry)
        };
        var currentEvent = eventList.FirstOrDefault(e => currentDay >= e.Item1 && currentDay <= e.Item2).Item3;
        Console.WriteLine($"Current day: {currentDay % 31} {(int)currentDay / 31} Current event: {currentEvent}");

        return a => currentEvent == eventVal;
    }

    private int GetCurrentDay()
    {
        return (int)((Now - new DateTime(2019, 6, 11, 17, 55, 0, DateTimeKind.Utc)).TotalDays / (TimeSpan.FromDays(5) + TimeSpan.FromHours(4)).TotalDays * 31 * 12) % (31 * 12);
    }
}
