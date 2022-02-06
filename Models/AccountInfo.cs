using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using hypixel;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class AccountInfo
    {
        [DataMember(Name = "userId")]
        public int UserId;
        [DataMember(Name = "mcIds")]
        public List<string> McIds = new List<string>();

        [DataMember(Name = "conIds")]
        public HashSet<string> ConIds = new HashSet<string>();

        [DataMember(Name = "tier")]
        public AccountTier Tier;
        [DataMember(Name = "expires")]
        public DateTime ExpiresAt;
    }
}