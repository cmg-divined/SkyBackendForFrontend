
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("How much the item usually sells for without any modifiers")]
public class CleanCostDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override FilterType FilterType => FilterType.NUMERICAL;

    protected override Expression<Func<FlipInstance, double>> GetSelector(FilterContext filters)
    {
        return f => f.Context.ContainsKey("cleanCost") ? double.Parse(f.Context["cleanCost"]) : 0;
    }
}