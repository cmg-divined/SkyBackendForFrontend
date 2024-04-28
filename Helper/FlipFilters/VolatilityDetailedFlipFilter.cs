
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Normalized volatility, median changes from 20 to 19 are 2, from 20 to 5 would be 74")]
public class VolatilityDetailedFlipFilter : NumberDetailedFlipFilter
{
    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return flip => flip.Auction.Context.ContainsKey("volat") ? double.Parse(flip.Auction.Context["volat"]) : 0;
    }
}