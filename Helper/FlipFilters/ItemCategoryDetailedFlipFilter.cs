
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;
using Coflnet.Sky.Items.Client.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Commands.Shared
{
    public class ItemCategoryDetailedFlipFilter : DetailedFlipFilter
    {
        public object[] Options => Enum.GetValues<ItemCategory>().Select(t=> (object)t).ToArray();

        public FilterType FilterType => FilterType.Equal;

        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
        {
            if (!Enum.TryParse<ItemCategory>(val, true, out ItemCategory itemCategory))
                throw new CoflnetException("invalid_category", $"the specified category {val} does not exist");
            var response = DiHandler.ServiceProvider.GetService<Sky.Items.Client.Api.IItemsApi>().ItemsCategoryCategoryItemsGet(itemCategory).ToHashSet();
            return flip => response.Contains(flip.Auction.Tag);
        }
    }
}