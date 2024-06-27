
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    [FilterDescription("Selects estimated profit of flip")]
    public class ProfitDetailedFlipFilter : NumberDetailedFlipFilter
    {
        protected override Expression<Func<FlipInstance, double>> GetSelector(FilterContext filters)
        {
            return (f) => f.Profit;
        }
    }
}