
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

public class DoNotOpenDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => new object[] { "true" };

    public FilterType FilterType => FilterType.BOOLEAN;

    public Expression<Func<FlipInstance, bool>> GetExpression(FilterContext filters, string val)
    {
        return f => f.Context.TryAdd("notOpen", string.Empty);
    }
}
