
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Targets flips found by prem+ instance, marked with gray !")]
public class PremPlusDetailedFlipFilter : BoolDetailedFlipFilter
{
    public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool expected)
    {
        return flip => (flip.Context != null && flip.Context.ContainsKey("bfcs")) == expected;
    }
}
