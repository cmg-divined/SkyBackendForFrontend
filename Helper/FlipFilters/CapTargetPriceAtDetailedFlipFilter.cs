
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Cap target price at a certain value, not sure why but you can")]
public class CapTargetPriceAtDetailedFlipFilter : NumberDetailedFlipFilter
{

    public override FilterType FilterType => FilterType.NUMERICAL;

    public override Expression<Func<FlipInstance, bool>> GetExpression(FilterContext filters, string val)
    {
        var target = NumberParser.Long(val);
        return f => f.Context.TryAdd("target", Math.Min(f.Target, target).ToString());
    }
}
