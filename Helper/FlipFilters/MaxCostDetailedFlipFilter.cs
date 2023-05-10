
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    [FilterDescription("Maximum cost of an auction")]
    public class MaxCostDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new string[] { "0", 10_000_000_000.ToString() };

        public FilterType FilterType => FilterType.NUMERICAL;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            var max = NumberParser.Long(val);
            return flip => flip.Auction.StartingBid < max;
        }
    }
}