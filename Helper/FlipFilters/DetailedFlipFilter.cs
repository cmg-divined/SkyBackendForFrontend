
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    public interface DetailedFlipFilter
    {
        Expression<Func<FlipInstance, bool>> GetExpression(Dictionary<string, string> filters, string val);
        object[] Options {get;}
    }
    

}