
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class MaxCostDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => throw new NotImplementedException();

        public FilterType FilterType => throw new NotImplementedException();

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            var max = NumberParser.Long(val);
            return flip => flip.Auction.StartingBid < max;
        }
    }
}