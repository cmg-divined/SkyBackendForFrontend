
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
public class IntroductionAgeDaysDetailedFlipFilter : DetailedFlipFilter
{
    public FilterType FilterType => FilterType.LOWER | FilterType.Equal;

    public object[] Options => new object[] { 1, 14 };

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        if (!int.TryParse(val, out int days))
            throw new CoflnetException("invalid_days", $"the specified days {val} is not a number");
        var service = DiHandler.GetService<FilterStateService>();
        var state = service.State;
        if(ItemDetails.Instance.TagLookup.Count > 10 && state.ExistingTags.Count == 0)
        {
            // for very new items check against known items on startup
            state.ExistingTags = new HashSet<string>(ItemDetails.Instance.TagLookup.Keys);
        }
        
        var items = service.GetIntroductionAge(days);
        return flip => items.Contains(flip.Auction.Tag) || !state.ExistingTags.Contains(flip.Auction.Tag);
    }
}