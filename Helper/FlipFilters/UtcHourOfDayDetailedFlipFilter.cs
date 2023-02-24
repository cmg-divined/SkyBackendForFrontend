
using System;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared;

public class UtcHourOfDayDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override object[] Options => new object[] { 0, 23 };

    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => DateTime.UtcNow.Hour;
    }
}
