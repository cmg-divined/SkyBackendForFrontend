
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{

    public class VolumeDetailedFlipFilter : NumberDetailedFlipFilter
    {
        public override object[] Options => new object[]{0.05,1000};

        protected override Expression<Func<FlipInstance, double>> GetSelector()
        {
            return (f) => f.Volume;
        }
    }
}