
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
    
    private (string val, DateTime lastUpdate) lastUpdate;

    public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val)
    {
        // normalize the name
        val = Options.FirstOrDefault(t => t.ToString().ToLower() == val.ToLower())?.ToString();
        if (val == null)
            throw new CoflnetException("invalid_mayor", "The specified mayor does not exist");
        string current = CurrentMayor();
        return (f) => val == current;
    }

    public string CurrentMayor()
    {
        if (DateTime.Now - lastUpdate.lastUpdate > TimeSpan.FromMinutes(2))
        {
            // update as too old
            lastUpdate = (TargetMayor(), DateTime.Now);
        }
        var current = lastUpdate.val;
        if (current == null)
            throw new CoflnetException("no_mayor", "Current mayor could not be retrieved");
        return current;
    }

    protected virtual string TargetMayor()
    {
        var response = DiHandler.GetService<Sky.Mayor.Client.Api.IMayorApi>().MayorCurrentGetWithHttpInfo();
        return JsonConvert.DeserializeObject<ModelCandidate>(response.Data.ToString()).Name;
    }

    public Filter.FilterType FilterType => Filter.FilterType.Equal;
}
