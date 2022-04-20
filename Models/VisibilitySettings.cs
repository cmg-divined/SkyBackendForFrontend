using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class VisibilitySettings
    {
        [DataMember(Name = "cost")]
        [SettingsDoc("Show the cost of a flip")]
        public bool Cost;
        [DataMember(Name = "estProfit")]
        [SettingsDoc("Estimated profit, based on estimated sell -(ah tax)")]
        public bool EstimatedProfit;
        [SettingsDoc("Show closest lowest bin (adds a few ms)")]
        [DataMember(Name = "lbin")]
        public bool LowestBin;
        [DataMember(Name = "slbin")]
        [SettingsDoc("Second lowest bin (adds a few ms)")]
        public bool SecondLowestBin;
        [DataMember(Name = "medPrice")]
        [SettingsDoc("Show median/target price, equals lbin if sniper")]
        public bool MedianPrice;
        [DataMember(Name = "seller")]
        [SettingsDoc("Show the sellers name (adds a few ms)")]
        public bool Seller;
        [DataMember(Name = "volume")]
        [SettingsDoc("Show the average sell volume in 24 hours")]
        public bool Volume;
        [DataMember(Name = "extraFields")]
        [SettingsDoc("How many extra information fields to display below the flip")]
        public int ExtraInfoMax;
        [DataMember(Name = "avgSellTime")]
        [SettingsDoc("Show estimated sell time (not a thing yet)", true)]
        public bool AvgSellTime;
        [DataMember(Name = "profitPercent")]
        [SettingsDoc("Show profit percentage")]
        public bool ProfitPercentage;
        [DataMember(Name = "profit")]
        [SettingsDoc("Show absolute amount of profit")]
        public bool Profit;

        [DataMember(Name = "sellerOpenBtn")]
        [SettingsDoc("Display a button to open the sellers ah")]
        public bool SellerOpenButton;
        [DataMember(Name = "lore")]
        [SettingsDoc("Show the item description in hover text")]
        public bool Lore;
        [DataMember(Name = "links")]
        [SettingsDoc("Enables/disables links (website setting)", true)]
        public bool CopyLinkOnFlipClick;
        [DataMember(Name = "copySuccessMessage")]
        [SettingsDoc("shows/hides copy message (website setting)", true)]
        public bool CopySuccessMessage;
        [DataMember(Name = "hideSold")]
        [SettingsDoc("shows/hides sold auctions (website setting)", true)]
        public bool HideSoldAuction;
    }
}