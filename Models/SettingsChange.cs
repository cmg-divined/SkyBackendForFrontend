using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class SettingsChange
    {
        [DataMember(Name = "version")]
        public int Version;
        [DataMember(Name = "settings")]
        public FlipSettings Settings = new FlipSettings();
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
        [IgnoreDataMember]
        public IEnumerable<long> LongConIds => ConIds.Select(id =>
        {
            try
            {
                return BitConverter.ToInt64(Convert.FromBase64String(id.Replace('_', '/').Replace('-', '+')));
            }
            catch (Exception)
            {
                Console.WriteLine("invalid conid: " + id);
                return new Random().Next();
            }
        });
    }
}
