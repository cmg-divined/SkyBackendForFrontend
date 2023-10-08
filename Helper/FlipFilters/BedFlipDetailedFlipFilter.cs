
using System;
using System.Linq.Expressions;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    [FilterDescription("Flips still 20 seconds in grace period")]
    public class BedFlipDetailedFlipFilter : BoolDetailedFlipFilter
    {
        public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool expected)
        {
            return flip => (flip.Auction.Start + TimeSpan.FromSeconds(20) > DateTime.Now) == expected;
        }
    }
}