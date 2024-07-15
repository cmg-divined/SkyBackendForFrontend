using System;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;
using MessagePack;

namespace Coflnet.Sky.Commands.Shared;
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
        Name = item.Name;
        Id = item.Tag;
        Type = "item";
        var isPet = IsPet(item);
        if (item.Tag != null && !item.Tag.StartsWith("POTION") && !isPet && !item.Tag.StartsWith("RUNE"))
            if (item.Tag.StartsWith("ENCHANTMENT_"))
                IconUrl = "https://sky.coflnet.com/static/icon/ENCHANTED_BOOK";
            else
                IconUrl = "https://sky.coflnet.com/static/icon/" + item.Tag;
        else
            IconUrl = item.IconUrl;
        if (isPet && !Name.Contains("Pet") && Name != null)
            Name += " Pet";

        HitCount = item.HitCount + ITEM_EXTRA_IMPORTANCE;
        if (ItemReferences.RemoveReforgesAndLevel(Name) != Name)
            HitCount -= NOT_NORMALIZED_PENILTY;
        Tier = item.Tier;
    }

    public SearchResultItem(Api.Client.Model.SearchResultItem item)
    {
        Name = item.Name;
        IconUrl = item.IconUrl;
        Image = item.Img;
        Name = item.Name;
        Enum.TryParse<Tier>(item.Tier.ToString(), true, out Tier);
        Type = item.Type;
        Id = item.Id;
    }

    private static bool IsPet(ItemDetails.ItemSearchResult item)
    {
        return (item?.Tag?.StartsWith("PET") ?? false) && !item.Tag.StartsWith("PET_SKIN") && !item.Tag.StartsWith("PET_ITEM");
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
        Name = player.Name;
        Id = player.UUid;
        IconUrl = SearchService.PlayerHeadUrl(player.UUid);
        Type = "player";
        HitCount = player.HitCount;
    }
}
