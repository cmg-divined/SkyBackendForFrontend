
using System;
using System.Linq.Expressions;

namespace Coflnet.Sky.Commands.Shared
{
    public class FraggedDetailedFlipFilter : BoolDetailedFlipFilter
    {
        public override Expression<Func<FlipInstance, bool>> GetStateExpression(bool target)
        {
            return a => a.Tag.StartsWith("STARRED_");
        }
    }
}