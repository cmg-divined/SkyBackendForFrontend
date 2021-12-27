
using System.Collections.Generic;
using System.Linq;
using Coflnet.Sky.Filter;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFilter
    {
        private static FilterEngine FilterEngine = new FilterEngine();

        private Dictionary<string, string> Filters;

        public FlipFilter(Dictionary<string, string> filters)
        {
            Filters = filters;
        }

        public bool IsMatch(FlipInstance flip)
        {
            return Filters == null || FilterEngine.AddFilters(new SaveAuction[] { flip.Auction }.AsQueryable(), Filters, false).Any();
        }
    }
    
}