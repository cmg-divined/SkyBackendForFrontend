using System;

namespace Coflnet.Sky.Commands.Shared
{
    /// <summary>
    /// Details about a single flip
    /// </summary>
    public class FlipDetails
    {
        public string ItemName;
        public string ItemTag;
        public string Tier;
        public long PricePaid;
        public long SoldFor;
        public LowPricedAuction.FinderType Finder;
        public long uId;
        public string OriginAuction;
        public string SoldAuction;
        public DateTime BuyTime;
        public DateTime SellTime;
        /// <summary>
        /// Profit of this flip (takes property changes into account)
        /// </summary>
        public long Profit;
    }

}