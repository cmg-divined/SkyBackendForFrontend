
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public abstract class NoRangeBase : DetailedFlipFilter
    {
        public virtual object[] Options => new object[] { 1, 100_000_000 };

        public virtual FilterType FilterType => FilterType.NUMERICAL | FilterType.LOWER;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            AssertNoRange(val);
            var numeric = NumberParser.Long(val);
            return GetNumExpression(numeric);
        }

        private void AssertNoRange(string val)
        {
            if (!val.Contains('>') && !val.Contains('<') && !val.Contains('-'))
                return;
            var name = this.GetType().Name.Replace("DetailedFlipFilter", "").Replace("Min", "").Replace("Max", "");
            throw new CoflnetException("no_ranges", $"This filter doesn't support ranges, use the `{name}` filter instead");
        }

        public abstract Expression<Func<FlipInstance, bool>> GetNumExpression(long val);
    }
}