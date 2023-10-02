
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Commands.Shared;
public class IntroductionAgeDaysDetailedFlipFilter : DetailedFlipFilter
{
    public FilterType FilterType => FilterType.RANGE | FilterType.Equal;

    public object[] Options => new object[] { 1, 20 };
    private HashSet<string> knownExisting;


    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        if (!int.TryParse(val, out int days))
            throw new CoflnetException("invalid_days", $"the specified days {val} is not a number");
        if(ItemDetails.Instance.TagLookup.Count > 10 && knownExisting == null)
        {
            // for very new items check against known items on startup
            knownExisting = new HashSet<string>(ItemDetails.Instance.TagLookup.Keys);
        }
        var items = DiHandler.ServiceProvider.GetService<Sky.Items.Client.Api.IItemsApi>().ItemsRecentGet(days);
        return flip => items.Contains(flip.Auction.Tag) || !knownExisting.Contains(flip.Auction.Tag);
    }
}