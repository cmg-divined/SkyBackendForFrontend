
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Filters marked with this are copied over to imported configs")]
public class KeepOnImportDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => [true];
    public FilterType FilterType => FilterType.BOOLEAN;

    public Expression<Func<FlipInstance, bool>> GetExpression(FilterContext filters, string val)
    {
        return f => true;
    }
}