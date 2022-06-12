using System;
using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
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

        public override bool Equals(object obj)
        {
            return obj is UserMute mute &&
                   Uuid == mute.Uuid;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Uuid);
        }
    }
}