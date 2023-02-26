
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
[FilterDescription("Profit per amount. 128coins / 64 = 2")]
public class ProfitPerUnitDetailedFlipFilter : NumberDetailedFlipFilter
{
    protected override Expression<Func<FlipInstance, double>> GetSelector()
    {
        return (f) => f.Profit / Math.Max(f.Auction.Count, 1);
    }
}