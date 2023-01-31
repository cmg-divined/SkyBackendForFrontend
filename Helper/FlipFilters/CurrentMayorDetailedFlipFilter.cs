
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using System.Linq;

namespace Coflnet.Sky.Commands.Shared;
public class CurrentMayorDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => new object[] {
            "Aatrox", "Cole", "Diana", "Diaz", "Foxy", "Finnegan", "Marina", "Paul", "Derpy", "Jerry", "Scorpius", "Dante", "Barry"
            };

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        // normalize the name
        val = Options.FirstOrDefault(t => t.ToString().ToLower() == val.ToLower()).ToString();
        if (val == null)
            throw new CoflnetException("invalid_mayor", "The specified mayor does not exist");
        var current = DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorCurrentGet();
        if (current == null || current.Name == null)
            throw new CoflnetException("no_mayor", "Current mayor could not be retrieved");
        return (f) => val == current.Name;
    }
    public Filter.FilterType FilterType => Filter.FilterType.SIMPLE;
}