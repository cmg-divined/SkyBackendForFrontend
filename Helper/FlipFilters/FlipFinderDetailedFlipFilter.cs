
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFinderDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options =>  Enum.GetValues<LowPricedAuction.FinderType>().Select(t=> (object)t).ToArray();

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            if (!Enum.TryParse<LowPricedAuction.FinderType>(val, true, out LowPricedAuction.FinderType targetType))
                throw new CoflnetException("invalid_finder", "the specified finder {val} does not exist");
            return flip => flip.Finder == targetType;
        }
    }


}