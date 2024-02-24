
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Removes filter from settings after the specified time")]
public class RemoveAfterDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => new object[] { "" };

    public FilterType FilterType => FilterType.DATE;

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        return x => true;
    }
}
