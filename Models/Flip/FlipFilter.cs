
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFilter
    {
        private static FilterEngine FilterEngine = new FilterEngine();

        private Func<SaveAuction, bool> Filters;
        private Func<FlipInstance, bool> FlipFilters = null;

        private static ClassNameDictonary<DetailedFlipFilter> additionalFilters = new();

        static FlipFilter()
        {
            additionalFilters.Add<MinProfitDetailedFlipFilter>();
            additionalFilters.Add<VolumeDetailedFlipFilter>();
            additionalFilters.Add<MinProfitPercentageDetailedFlipFilter>();
            additionalFilters.Add<FlipFinderDetailedFlipFilter>();
        }

        public FlipFilter(Dictionary<string, string> filters)
        {
            Expression<Func<FlipInstance, bool>> expression = null;
            if (filters != null)
                foreach (var item in additionalFilters.Keys)
                {
                    var match = filters.Where(f=>f.Key.ToLower() == item).FirstOrDefault();
                    if (match.Key != default)
                    {
                        filters.Remove(match.Key);
                        expression = additionalFilters[item].GetExpression(filters, match.Value);
                        Console.WriteLine("set expression " + expression.ToString());
                    }
                }
            Filters = FilterEngine.GetMatcher(filters);
            if (expression != null)
                FlipFilters = expression.Compile();
        }

        public bool IsMatch(FlipInstance flip)
        {
            return Filters == null || Filters(flip.Auction) && (FlipFilters == null || FlipFilters(flip));
        }

        public Expression<Func<FlipInstance, bool>> GetExpression()
        {
            if (Filters == null && FlipFilters == null)
                return f => true;
            if (FlipFilters == null)
                return f => Filters(f.Auction);
            if (Filters == null)
                return flip => FlipFilters(flip);
            Console.WriteLine("hmmm ");
            return flip => Filters(flip.Auction) && FlipFilters(flip);
        }
    }

}