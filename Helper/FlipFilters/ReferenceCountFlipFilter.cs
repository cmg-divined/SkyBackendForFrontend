
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Count of references used for flip, together with high volume indicates that item is new and mayb be volatilie")]
public class ReferenceCountDetailedFlipFilter : NumberDetailedFlipFilter
{
    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return flip => flip.Auction.Context.ContainsKey("refCount") ? double.Parse(flip.Auction.Context["refCount"]) : 0;
    }
}