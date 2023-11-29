using System;
using System.Collections.Generic;
using Coflnet.Sky.Core;
using Newtonsoft.Json.Converters;

namespace Coflnet.Sky.Commands.Shared
{
    [Flags]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))] 
    public enum FlipFlags
    {
        None = 0,
        DifferentBuyer = 1,
        ViaTrade = 2,
        /// <summary>
        /// More than one item was traded, not exact price
        /// </summary>
        MultiItemTrade = 4,
    }
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
        /// <summary>
        /// A list of changes that were applied to the item
        /// </summary>
        public List<PropertyChange> PropertyChanges;
        public FlipFlags Flags;
    }

    public class PropertyChange
    {
        public string Description;
        public long Effect;

        public PropertyChange()
        {
        }

        public PropertyChange(string description, long effect)
        {
            Description = description;
            Effect = effect;
        }


    }
}