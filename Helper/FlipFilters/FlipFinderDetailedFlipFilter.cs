
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFinderDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => new LowPricedAuction.FinderType[]{
            LowPricedAuction.FinderType.FLIPPER,
            LowPricedAuction.FinderType.SNIPER,
            LowPricedAuction.FinderType.SNIPER_MEDIAN,
            LowPricedAuction.FinderType.USER,
            LowPricedAuction.FinderType.FLIPPER_AND_SNIPERS,
            LowPricedAuction.FinderType.SNIPERS,
        }.Select(t=> (object)t).ToArray();

        public FilterType FilterType => FilterType.Equal;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            if (!Enum.TryParse<LowPricedAuction.FinderType>(val, true, out LowPricedAuction.FinderType targetType))
                throw new CoflnetException("invalid_finder", "the specified finder {val} does not exist");
            return flip => targetType.HasFlag(flip.Finder);
        }
    }
}