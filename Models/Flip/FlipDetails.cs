namespace Coflnet.Sky.Commands.Shared
{
    /// <summary>
    /// Details about a single flip
    /// </summary>
    public class FlipDetails
    {
        public string ItemName;
        public long PricePaid;
        public long SoldFor;
        public LowPricedAuction.FinderType Finder;
        public long uId;
        public string OriginAuction;
        public string SoldAuction;
    }

}