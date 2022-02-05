
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public class NumberDetailedFlipFinder : DetailedFlipFilter
    {
        public virtual object[] Options => new object[]{1,100_000_000};
        public Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string content)
        {
            var selector = GetSelector();
            if (content.Contains("-"))
            {
                var parts = content.Split("-").Select(a => long.Parse(a)).ToArray();
                var min = parts[0];
                var max = parts[1];
                return Sky.Filter.NumberFilter.ExpressionMinMax(selector, min, max);
            }
            var value = long.Parse(content.Replace("<", "").Replace(">", ""));
            if (content.StartsWith("<"))
                return Sky.Filter.NumberFilter.ExpressionMinMax(selector, 1, value - 1);
            if (content.StartsWith(">"))
            {
                return Sky.Filter.NumberFilter.ExpressionMinMax(selector, value, 1000);
            }

            return Sky.Filter.NumberFilter.ExpressionMinMax(selector, value, value);
            //return flip => flip.ProfitPercentage > min;
        }

        protected virtual Expression<Func<FlipInstance, long>> GetSelector()
        {
            return (f) => (long)f.Volume;
        }
    }
    

}