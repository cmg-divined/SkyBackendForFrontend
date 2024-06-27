
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
[FilterDescription("Ah category the item is in")]
public class AhCategoryDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => Enum.GetValues<Category>().Select(t => (object)t).ToArray();

    public FilterType FilterType => FilterType.Equal;

    public Expression<Func<FlipInstance, bool>> GetExpression(FilterContext filters, string val)
    {
        if (!Enum.TryParse<Category>(val, true, out Category ahCategory))
            throw new CoflnetException("invalid_category", $"the specified category {val} does not exist");

        return flip => flip.Auction.Category == ahCategory;
    }
}
