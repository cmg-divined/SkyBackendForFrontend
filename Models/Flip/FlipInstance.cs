using System.Collections.Generic;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class FlipInstance
    {
        [DataMember(Name = "median")]
        public long MedianPrice;
        [DataMember(Name = "cost")]
        public long LastKnownCost => (Auction.HighestBidAmount == 0 ? Auction.StartingBid : Auction.HighestBidAmount);
        [DataMember(Name = "uuid")]
        public string Uuid;
        [DataMember(Name = "name")]
        public string Name;
        [DataMember(Name = "sellerName")]
        public string SellerName;
        [DataMember(Name = "volume")]
        public float Volume;
        [DataMember(Name = "tag")]
        public string Tag;
        [DataMember(Name = "bin")]
        public bool Bin;
        [DataMember(Name = "sold")]
        public bool Sold { get; set; }
        [DataMember(Name = "tier")]
        public Tier Rarity { get; set; }
        [DataMember(Name = "prop")]
        public List<string> Interesting { get; set; }
        [DataMember(Name = "secondLowestBin")]
        public long? SecondLowestBin { get; set; }

        [DataMember(Name = "lowestBin")]
        public long? LowestBin;
        [DataMember(Name = "auction")]
        public SaveAuction Auction;
        [IgnoreDataMember]
        public long UId => AuctionService.Instance.GetId(this.Uuid);
        [IgnoreDataMember]
        [Newtonsoft.Json.JsonProperty]
        public long Profit
        {
            get
            {
                var targetPrice = (Finder == LowPricedAuction.FinderType.SNIPER ? LowestBin : MedianPrice);
                var reduction = 2f;
                if (targetPrice > 10_000_000)
                    reduction = 3;
                if (targetPrice > 100_000_000)
                    reduction = 3.5f;
                return (long)(targetPrice * (100 - reduction) / 100 - LastKnownCost);
            }
        }

        [IgnoreDataMember]
        public long ProfitPercentage => (Profit * 100 / (LastKnownCost == 0 ? int.MaxValue : LastKnownCost));
        [IgnoreDataMember]
        public long Target => (Finder == LowPricedAuction.FinderType.SNIPER ? LowestBin : MedianPrice) ?? MedianPrice;

        [DataMember(Name = "context")]
        public Dictionary<string, string> Context { get; set; }

        [DataMember(Name = "finder")]
        public LowPricedAuction.FinderType Finder;
    }
}
