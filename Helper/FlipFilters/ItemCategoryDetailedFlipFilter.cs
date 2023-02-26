
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Items.Client.Model;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Coflnet.Sky.Commands.Shared
{
    public class ItemCategoryDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => Enum.GetValues<ItemCategory>().Select(t => (object)t).ToArray();

        public FilterType FilterType => FilterType.Equal;

        private static readonly ConcurrentDictionary<ItemCategory, HashSet<string>> categoryToTags = new ();
        private static DateTime lastUpdate = DateTime.MinValue;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            if (!Enum.TryParse<ItemCategory>(val, true, out ItemCategory itemCategory))
                throw new CoflnetException("invalid_category", $"the specified category {val} does not exist");

            if (DateTime.Now - lastUpdate > TimeSpan.FromHours(2))
            {
                categoryToTags.Clear();
                lastUpdate = DateTime.Now;
            }
            if (categoryToTags.ContainsKey(itemCategory))
            {
                var tags = categoryToTags[itemCategory];
                return flip => tags.Contains(flip.Auction.Tag);
            }
            var response = DiHandler.ServiceProvider.GetService<Sky.Items.Client.Api.IItemsApi>().ItemsCategoryCategoryItemsGet(itemCategory).ToHashSet();
            if (response.Count == 0)
                throw new CoflnetException("no_items", $"No items found for category {itemCategory}");
            categoryToTags[itemCategory] = response;
            return flip => response.Contains(flip.Auction.Tag);
        }
    }
}