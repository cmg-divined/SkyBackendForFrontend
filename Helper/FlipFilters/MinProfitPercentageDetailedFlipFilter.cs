
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
[FilterDescription("Minimum profit percent (%) of a flip")]
public class MinProfitPercentageDetailedFlipFilter : NoRangeBase
{
    public override Expression<Func<FlipInstance, bool>> GetNumExpression(long val)
    {
        return flip => flip.ProfitPercentage > val;
    }
}