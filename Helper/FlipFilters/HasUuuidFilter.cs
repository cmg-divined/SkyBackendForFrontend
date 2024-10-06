
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Item has a uuid and is stackable or not")]
public class HasUuuidFilter : BoolDetailedFlipFilter
{
    public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool expected)
    {
        return flip => flip.Auction.FlatenedNBT.ContainsKey("uuid") == expected;
    }
}