
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Coflnet.Sky.Core;
using System.Linq;
using Coflnet.Sky.Mayor.Client;
using Newtonsoft.Json;
using Coflnet.Sky.Mayor.Client.Model;

namespace Coflnet.Sky.Commands.Shared;
public class CurrentMayorDetailedFlipFilter : DetailedFlipFilter
{
    public object[] Options => new object[] {
            "Aatrox", "Cole", "Diana", "Diaz", "Foxy", "Finnegan", "Marina", "Paul", "Derpy", "Jerry", "Scorpius", "Dante", "Barry"
            };

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        // normalize the name
        val = Options.FirstOrDefault(t => t.ToString().ToLower() == val.ToLower())?.ToString();
        if (val == null)
            throw new CoflnetException("invalid_mayor", "The specified mayor does not exist");
        var current = TargetMayor();
        if (current == null || current == null)
            throw new CoflnetException("no_mayor", "Current mayor could not be retrieved");
        return (f) => val == current;
    }

    protected virtual string TargetMayor()
    {
        var response = DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorCurrentGetWithHttpInfo();
        return JsonConvert.DeserializeObject<ModelCandidate>(response.Data.ToString()).Name;
    }

    public Filter.FilterType FilterType => Filter.FilterType.Equal;
}
