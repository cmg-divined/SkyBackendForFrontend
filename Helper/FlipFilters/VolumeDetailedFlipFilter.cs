
using System;
using System.Linq.Expressions;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{

    public class VolumeDetailedFlipFilter : NumberDetailedFlipFilter
    {
        public override object[] Options => new object[]{1,1000};

        protected override Expression<Func<FlipInstance, long>> GetSelector()
        {
            return (f) => (long)f.Volume;
        }
    }
    

}