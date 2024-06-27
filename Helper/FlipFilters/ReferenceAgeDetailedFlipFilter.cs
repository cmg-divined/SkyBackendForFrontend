
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;
[FilterDescription("The maximum age of the 3rd most recent reference (short term median)")]
public class ReferenceAgeDetailedFlipFilter : NumberDetailedFlipFilter
{
    protected override Expression<Func<FlipInstance, double>> GetSelector(FilterContext filters)
    {
        return (f) => f.Context.ContainsKey("refAge") ? double.Parse(f.Context["refAge"]) : 7;
    }
}