
using System;
using System.Collections.Generic;
using System.Linq;
using Coflnet.Sky.Filter;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFilter
    {
        private static FilterEngine FilterEngine = new FilterEngine();

        private Func<SaveAuction,bool> Filters;

        public FlipFilter(Dictionary<string, string> filters)
        {
            Filters = FilterEngine.GetMatcher(filters);
        }

        public bool IsMatch(FlipInstance flip)
        {
            return Filters == null || Filters(flip.Auction);
        }
    }
    
}