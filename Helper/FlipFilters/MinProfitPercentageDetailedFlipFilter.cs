
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class MinProfitPercentageDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new object[]{1,100_000_000};
        public FilterType FilterType => FilterType.NUMERICAL | FilterType.LOWER | FilterType.RANGE;
        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            var min = NumberParser.Long(val);
            return flip => flip.ProfitPercentage > min;
        }
    }
}