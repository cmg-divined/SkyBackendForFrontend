
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
[FilterDescription("Minimum profit of a flip")]
public class MinProfitDetailedFlipFilter : NoRangeBase
{
    public override Expression<Func<FlipInstance, bool>> GetNumExpression(long val)
    {
        return flip => flip.Profit > val;
    }
}