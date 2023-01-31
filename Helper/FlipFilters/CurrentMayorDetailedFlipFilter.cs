
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared;
public class CurrentMayorDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => new object[] {
            "Aatrox", "Cole", "Diana", "Diaz", "Foxy", "Finnegan", "Marina", "Paul", "Derpy", "Jerry", "Scorpius", "Dante", "Barry"
            };

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        var current = DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorCurrentGet();
        return (f) => val == current.Name;
    }
    public Filter.FilterType FilterType => Filter.FilterType.SIMPLE;
}