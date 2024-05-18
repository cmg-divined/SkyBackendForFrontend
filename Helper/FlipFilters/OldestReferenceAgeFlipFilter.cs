
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("How many days ago the oldest reference used for anit market manipulation was sold. 0 is today")]
public class OldestReferenceAgeDetailedFlipFilter : NumberDetailedFlipFilter
{
    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return flip => flip.Auction.Context.ContainsKey("oldRef") ? double.Parse(flip.Auction.Context["oldRef"]) : 0;
    }
}