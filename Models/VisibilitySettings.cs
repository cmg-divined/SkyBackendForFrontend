using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class VisibilitySettings
    {
        [DataMember(Name = "cost")]
        public bool Cost;
        [DataMember(Name = "estProfit")]
        public bool EstimatedProfit;
        [DataMember(Name = "lbin")]
        public bool LowestBin;
        [DataMember(Name = "slbin")]
        public bool SecondLowestBin;
        [DataMember(Name = "medPrice")]
        public bool MedianPrice;
        [DataMember(Name = "seller")]
        public bool Seller;
        [DataMember(Name = "volume")]
        public bool Volume;
        [DataMember(Name = "extraFields")]
        public int ExtraInfoMax;
        [DataMember(Name = "avgSellTime")]
        public bool AvgSellTime;
        [DataMember(Name = "profitPercent")]
        public bool ProfitPercentage;
        [DataMember(Name = "profit")]
        public bool Profit;

        [DataMember(Name = "sellerOpenBtn")]
        public bool SellerOpenButton;
        [DataMember(Name = "lore")]
        public bool Lore;
        /// <summary>
        /// this has been disabled in the mod by using {get;set,}
        /// </summary>
        [DataMember(Name = "links")]
        public bool CopyLinkOnFlipClick { get; set; }
        /// <summary>
        /// this has been disabled in the mod by using {get;set,}
        /// </summary>
        [DataMember(Name = "copySuccessMessage")]
        public bool CopySuccessMessage { get; set; }
        /// <summary>
        /// this has been disabled in the mod by using {get;set,}
        /// </summary>
        [DataMember(Name = "hideSold")]
        public bool HideSoldAuction { get; set; }
    }
}