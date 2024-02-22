
using System;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared;

public class PremPlusDetailedFlipFilter : BoolDetailedFlipFilter
{
    public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool expected)
    {
        return flip => (flip.Auction.Context != null && flip.Auction.Context.ContainsKey("bfcs")) == expected;
    }
}
