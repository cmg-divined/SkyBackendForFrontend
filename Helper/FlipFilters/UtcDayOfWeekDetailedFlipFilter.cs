
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Only enable filter in specific day of the week based on UTC time. Sunday is 0, Saturday is 6")]
public class UtcDayOfWeekDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override object[] Options => new object[] { 0, 6 };

    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => (int)DateTime.UtcNow.DayOfWeek;
    }
}