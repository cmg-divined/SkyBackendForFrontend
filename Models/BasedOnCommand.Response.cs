using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class BasedOnCommandResponse
    {
        [DataMember(Name = "uuid")]
        public string uuid;
        [DataMember(Name = "highestBid")]
        public long highestBid;
        [DataMember(Name = "end")]
        public System.DateTime end;
        [DataMember(Name = "name")]
        public string ItemName { get; set; }
    }

}