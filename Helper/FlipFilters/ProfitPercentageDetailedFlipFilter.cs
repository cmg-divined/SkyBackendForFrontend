
using System;
using System.Linq.Expressions;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class ProfitPercentageDetailedFlipFilter : NumberDetailedFlipFilter
    {
        protected override Expression<Func<FlipInstance, double>> GetSelector()
        {
            return (f) => (long)f.ProfitPercentage;
        }
    }
    

}