
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Only enable filter in specific hour(s) of the day based on UTC time. 8-12 includes 12:59")]
public class UtcHourOfDayDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override object[] Options => new object[] { 0, 23 };

    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => DateTime.UtcNow.Hour;
    }
}
