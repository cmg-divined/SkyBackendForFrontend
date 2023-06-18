
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipFilter
    {
        public static FilterEngine FilterEngine { get; } = new FilterEngine();

        private Func<SaveAuction, bool> Filters;
        private Func<FlipInstance, bool> FlipFilters = null;
        Expression<Func<FlipInstance, bool>> expression = null;

        public static CamelCaseNameDictionary<DetailedFlipFilter> AdditionalFilters { private set; get; } = new();

        public static IEnumerable<string> AllFilters => FilterEngine.AvailableFilters.Select(f => f.Name).Concat(AdditionalFilters.Keys);
        public static string FilterForName = "ForTag";

        static FlipFilter()
        {
            AdditionalFilters.Add<ForTagDetailedFlipFilter>();
            AdditionalFilters.Add<VolumeDetailedFlipFilter>();
            AdditionalFilters.Add<ProfitDetailedFlipFilter>();
            AdditionalFilters.Add<ProfitPerUnitDetailedFlipFilter>();
            AdditionalFilters.Add<ProfitPercentageDetailedFlipFilter>();
            AdditionalFilters.Add<FlipFinderDetailedFlipFilter>();
            AdditionalFilters.Add<BedFlipDetailedFlipFilter>();
            AdditionalFilters.Add<PreApiDetailedFlipFilter>();
            AdditionalFilters.Add<CurrentMayorDetailedFlipFilter>();
            AdditionalFilters.Add<LastMayorDetailedFlipFilter>();
            AdditionalFilters.Add<NextMayorDetailedFlipFilter>();
            AdditionalFilters.Add<DoNotOpenDetailedFlipFilter>();
            AdditionalFilters.Add<MinProfitPercentageDetailedFlipFilter>();
            AdditionalFilters.Add<ItemCategoryDetailedFlipFilter>();
            AdditionalFilters.Add<IntroductionAgeDaysDetailedFlipFilter>();
            AdditionalFilters.Add<ArmorSetDetailedFlipFilter>();
            AdditionalFilters.Add<ArmorSetNoHelmetDetailedFlipFilter>();
            AdditionalFilters.Add<MinProfitDetailedFlipFilter>();
            AdditionalFilters.Add<MaxCostDetailedFlipFilter>();
            AdditionalFilters.Add<ReferenceAgeDetailedFlipFilter>();
            AdditionalFilters.Add<ForceBlacklistDetailedFlipFilter>();
            AdditionalFilters.Add<AfterMainFilterDetailedFlipFilter>();
            AdditionalFilters.Add<PriorityOpenDetailedFlipFilter>();
            AdditionalFilters.Add<UtcHourOfDayDetailedFlipFilter>();
            AdditionalFilters.Add<UtcDayOfWeekDetailedFlipFilter>();
            AdditionalFilters.Add<CurrentEventDetailedFlipFilter>();
        }

        public FlipFilter(Dictionary<string, string> originalf)
        {
            if (originalf == null || originalf.Count == 0)
            {
                expression = f => true;
                return;
            }
            var filters = new Dictionary<string, string>(originalf);
            foreach (var item in AdditionalFilters.Keys)
            {
                var match = filters.Where(f => f.Key.ToLower() == item.ToLower()).FirstOrDefault();
                if (match.Key != default)
                {
                    try
                    {
                        filters.Remove(match.Key);
                        var newPart = AdditionalFilters[item].GetExpression(filters, match.Value);
                        if (expression == null)
                            expression = newPart;
                        else if (newPart != null)
                            expression = newPart.And(expression);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw new CoflnetException("filter_parsing", $"Error in filter {item} with value {match.Value} : {e.Message}");
                    }
                }
            }
            var filterExpression = FilterEngine.GetMatchExpression(filters);
            Expression<Func<FlipInstance, SaveAuction>> flipToAuction = f => f.Auction;
            var invoke = Expression.Invoke(filterExpression, flipToAuction.Body);
            Expression<Func<FlipInstance, bool>> auctionMatcher = Expression.Lambda<Func<FlipInstance, bool>>(invoke, flipToAuction.Parameters[0]);

            if (auctionMatcher == null)
                throw new Exception("matcher is null");
            if (expression == null)
                expression = auctionMatcher;
            else
                expression = auctionMatcher.And(expression);
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
            return expression;
        }
    }

}