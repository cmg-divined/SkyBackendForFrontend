using System.Collections.Generic;

namespace Coflnet.Sky.Commands.Shared
{
    public class FilterContext 
    {
        public Dictionary<string, string> filters;
        public IPlayerInfo playerInfo;

        public FilterContext(Dictionary<string, string> filters, IPlayerInfo playerInfo)
        {
            this.filters = filters;
            this.playerInfo = playerInfo;
        }

        public static implicit operator Dictionary<string, string>(FilterContext context)
        {
            return context.filters;
        }
    }

}