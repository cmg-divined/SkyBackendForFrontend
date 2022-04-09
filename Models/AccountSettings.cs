using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class AccountSettings
    {
        [DataMember(Name = "muted")]
        public List<UserMute> MutedUsers = new List<UserMute>();
    }

    [DataContract]
    public class UserMute
    {
        [DataMember(Name = "uuid")]
        public string Uuid;
        [DataMember(Name = "name")]
        public string OrigianlName;

        public UserMute(string uuid, string origianlName)
        {
            Uuid = uuid;
            OrigianlName = origianlName;
        }
    }
}