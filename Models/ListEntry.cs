
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;
using Coflnet.Sky.Filter;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class ListEntry
    {
        [DataMember(Name = "tag")]
        public string ItemTag;
        [DataMember(Name = "filter")]
        public Dictionary<string, string> filter;

        private Func<FlipInstance,bool> filterCache;

        public bool MatchesSettings(FlipInstance flip)
        {
            if (filterCache == null)
                filterCache = GetExpression().Compile();
            return filterCache(flip);
        }

        public Expression<Func<FlipInstance,bool>> GetExpression()
        {
            var filterCache = new FlipFilter(this.filter);
       //     Expression<Func<FlipInstance,bool>> normal = (flip) => (ItemTag == null || ItemTag == flip.Auction.Tag);
            return filterCache.GetExpression();
        }
    }
}