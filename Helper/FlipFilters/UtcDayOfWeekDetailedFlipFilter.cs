
using System;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared;

public class UtcDayOfWeekDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override object[] Options => new object[] { 0, 6 };

    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => (int)DateTime.UtcNow.DayOfWeek;
    }
}