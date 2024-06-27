
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
        [DataMember(Name = "displayName")]
        public string DisplayName;
        [DataMember(Name = "filter")]
        public Dictionary<string, string> filter;
        [DataMember(Name = "tags")]
        public List<string> Tags;
        [DataMember(Name = "order")]
        public int Order;
        [DataMember(Name = "group")]
        public string Group;
        [DataMember(Name = "disabled")]
        public bool Disabled;

        private Func<FlipInstance, bool> filterCache;

        public bool MatchesSettings(FlipInstance flip, IPlayerInfo playerInfo)
        {
            if (filterCache == null)
                filterCache = GetExpression(playerInfo).Compile();
            return (ItemTag == null || ItemTag == flip.Auction.Tag) && filterCache(flip);
        }

        public Expression<Func<FlipInstance, bool>> GetExpression(IPlayerInfo playerInfo = null)
        {
            if (Disabled)
                return f => false;
            var filterCache = new FlipFilter(filter, playerInfo);
            //     Expression<Func<FlipInstance,bool>> normal = (flip) => (ItemTag == null || ItemTag == flip.Auction.Tag);
            return filterCache.GetExpression();
        }

        public override bool Equals(object obj)
        {
            return obj is ListEntry entry &&
                   ItemTag == entry.ItemTag &&
                   DisplayName == entry.DisplayName &&
                   EqualityComparer<Dictionary<string, string>>.Default.Equals(filter, entry.filter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ItemTag, filter);
        }

        public ListEntry Clone()
        {
            return new ListEntry()
            {
                ItemTag = ItemTag,
                DisplayName = DisplayName,
                filter = filter == null ? null : new(filter),
                Tags = Tags == null ? null : new(Tags),
                Order = Order,
                Group = Group
            };
        }
    }
}