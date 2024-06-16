
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Cap target price at a certain value, not sure why but you can")]
public class CapTargetPriceAtDetailedFlipFilter : NumberDetailedFlipFilter
{
    public object[] Options => [1, 10_000_000_000];

    public FilterType FilterType => FilterType.NUMERICAL;

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        var target = NumberParser.Long(val);
        return f => f.Context.TryAdd("target", Math.Min(f.Target, target).ToString());
    }
}
