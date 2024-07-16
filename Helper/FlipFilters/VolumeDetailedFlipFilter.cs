
using System;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared
{

    public class VolumeDetailedFlipFilter : NumberDetailedFlipFilter
    {
        public override object[] Options => new object[] { 0.05, 1000 };

        protected override Expression<Func<FlipInstance, double>> GetSelector(FilterContext filters)
        {
            return (f) => f.Volume;
        }
    }
}