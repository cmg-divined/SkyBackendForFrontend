using System;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;
using MessagePack;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
        public class SearchResultItem
        {
            private const int ITEM_EXTRA_IMPORTANCE = 10;
            private const int NOT_NORMALIZED_PENILTY = ITEM_EXTRA_IMPORTANCE * 3 / 2;
            [DataMember(Name = "name")]
            public string Name;
            [DataMember(Name = "id")]
            public string Id;
            [DataMember(Name = "type")]
            public string Type;
            [DataMember(Name = "iconUrl")]
            public string IconUrl;
            /// <summary>
            /// Low resolution preview icon
            /// </summary>
            [DataMember(Name = "img")]
            public string Image;

            [DataMember(Name = "tier")]
            public Tier Tier;
            [IgnoreMember]
            //[Key("hits")]
            public int HitCount;

            public SearchResultItem() { }

            public SearchResultItem(ItemDetails.ItemSearchResult item)
            {
                this.Name = item.Name;
                this.Id = item.Tag;
                this.Type = "item";
                var isPet = IsPet(item);
                if (!item.Tag.StartsWith("POTION") && !isPet && !item.Tag.StartsWith("RUNE"))
                    IconUrl = "https://sky.shiiyu.moe/item/" + item.Tag;
                else
                    this.IconUrl = item.IconUrl;
                if (isPet && !Name.Contains("Pet") && Name != null)
                    this.Name += " Pet";

                this.HitCount = item.HitCount + ITEM_EXTRA_IMPORTANCE;
                if (ItemReferences.RemoveReforgesAndLevel(Name) != Name)
                    this.HitCount -= NOT_NORMALIZED_PENILTY;
                this.Tier = item.Tier;
            }

            private static bool IsPet(ItemDetails.ItemSearchResult item)
            {
                return ((item?.Tag?.StartsWith("PET") ?? false) && !item.Tag.StartsWith("PET_SKIN"));
            }

            public override bool Equals(object obj)
            {
                return obj is SearchResultItem item &&
                       Id == item.Id &&
                       Type == item.Type;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Type);
            }

            public SearchResultItem(PlayerResult player)
            {
                this.Name = player.Name;
                this.Id = player.UUid;
                this.IconUrl = SearchService.PlayerHeadUrl(player.UUid);
                this.Type = "player";
                this.HitCount = player.HitCount;
            }
        }
}