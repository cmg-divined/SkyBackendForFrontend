
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared;

[FilterDescription("Adjusts target price based on craft cost of ingredients multiplied by weight")]
public class CraftCostWeightDetailedFlipFilter : NumberDetailedFlipFilter
{
    public override FilterType FilterType => FilterType.RANGE;

    public override Expression<Func<FlipInstance, bool>> GetExpression(FilterContext filters, string val)
    {
        Dictionary<string, double> multipliers = ParseMultipliers(val);
        if (!multipliers.TryGetValue("default", out var defaultMultiplier))
            throw new ArgumentException("No default multiplier provided, use default:1 to disable");

        filters.filters.TryGetValue("MinProfit", out var minprofitString);
        NumberParser.TryLong(minprofitString, out var target);

        return f => f.Finder == LowPricedAuction.FinderType.CraftCost &&
            f.Context.ContainsKey("breakdown") &&
             f.Context.TryAdd("target", (JsonSerializer.Deserialize<Dictionary<string, long>>(f.Context["breakdown"], (JsonSerializerOptions)null)
                .Select(b => multipliers.GetValueOrDefault(b.Key, defaultMultiplier) * b.Value).Sum() + long.Parse(f.Context["cleanCost"])).ToString())
                && (target == 0 || long.Parse(f.Context["target"]) - f.Auction.StartingBid >= target)
                || f.Context.Remove("target") && false; // clear up temp stored
    }

    private static Dictionary<string, double> ParseMultipliers(string val)
    {
        try
        {
            return val.Split(',').ToDictionary(m => m.Split(':')[0], m => NumberParser.Double(m.Split(':')[1]));
        }
        catch (System.Exception e)
        {
            Console.WriteLine("craftcost filter: " + e);
            throw new CoflnetException("filter_parsing", $"Error in filter CraftCostWeight. Make sure to specify pairs separated by commas of like `modifier:multiplier,sharpness:0.7`");
        }
    }
}
