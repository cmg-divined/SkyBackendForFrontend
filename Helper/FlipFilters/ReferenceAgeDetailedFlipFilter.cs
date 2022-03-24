
using System;
using System.Linq.Expressions;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class ReferenceAgeDetailedFlipFilter : NumberDetailedFlipFilter
    {
        protected override Expression<Func<FlipInstance, double>> GetSelector()
        {
            return (f) => f.Context.ContainsKey("refAge") ? double.Parse(f.Context["refAge"]) : 7;
        }
    }

}