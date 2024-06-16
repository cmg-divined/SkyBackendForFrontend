
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Filter for the price estimations")]
public class TargetPriceDetailedFlipFilter : NumberDetailedFlipFilter
{
    public FilterType FilterType => FilterType.NUMERICAL;

    protected virtual Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => f.Target;
    }
}
