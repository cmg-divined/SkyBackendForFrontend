
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

public class DoNotRelistDetailedFlipFilter : DetailedFlipFilter
{
     public object[] Options => ["true"];

    public FilterType FilterType => FilterType.BOOLEAN;

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        return f => f.Context.TryAdd("noRelist", string.Empty);
    }
}
