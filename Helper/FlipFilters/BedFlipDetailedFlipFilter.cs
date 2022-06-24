
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    public class BedFlipDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new object[]{
            true,
            false
        };

        public FilterType FilterType => FilterType.Equal | FilterType.SIMPLE;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            var isBed = bool.Parse(val);
            return flip => (flip.Auction.Start + TimeSpan.FromSeconds(20) > DateTime.Now) == isBed;
        }
    }
}