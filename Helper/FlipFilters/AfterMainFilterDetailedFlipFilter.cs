
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    public class AfterMainFilterDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new object[] { "true" };

        public FilterType FilterType => FilterType.BOOLEAN | FilterType.SIMPLE;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            return null;
        }
    }
}