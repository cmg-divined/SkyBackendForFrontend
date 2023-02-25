
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    [FilterDescription("Prioritizes opening matching flips with the hotkey")]
    public class PriorityOpenDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new object[] { "true" };

        public FilterType FilterType => FilterType.BOOLEAN;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            return f => f.Context.TryAdd("priorityOpen", "t");
        }
    }
}