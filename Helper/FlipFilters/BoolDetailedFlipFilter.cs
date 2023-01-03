
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    public abstract class BoolDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new object[]{
            true,
            false
        };

        public FilterType FilterType => FilterType.Equal | FilterType.SIMPLE;

        public abstract Expression<Func<FlipInstance, bool>> GetStateExpression(bool target);

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            var expected = bool.Parse(val);
            return GetStateExpression(expected);
        }
    }
}