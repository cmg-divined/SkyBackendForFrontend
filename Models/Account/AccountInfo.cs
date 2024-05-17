using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class AccountInfo
    {
        [DataMember(Name = "userId")]
        public int UserIdOld;
        [DataMember(Name = "userIdString")]
        public string UserId;
        [DataMember(Name = "mcIds")]
        public HashSet<string> McIds = new HashSet<string>();

        [DataMember(Name = "conIds")]
        public HashSet<string> ConIds = new HashSet<string>();

        [DataMember(Name = "tier")]
        public AccountTier Tier;
        [DataMember(Name = "expires")]
        public DateTime ExpiresAt;
        [DataMember(Name = "activeCon")]
        public string ActiveConnectionId;

        [DataMember(Name = "lastCaptchaSolve")]
        public DateTime LastCaptchaSolve { get; set; }
        [DataMember(Name = "locale")]
        public string Locale { get; set; }
        [DataMember(Name = "timeZoneOffset")]
        public int TimeZoneOffset { get; set; }
        [DataMember(Name = "region")]
        public string Region { get; set; }
        [DataMember(Name = "captchaType")]
        public string CaptchaType { get; set; }
        [DataMember(Name = "badActionCount")]
        public int BadActionCount { get; set; }
        /// <summary>
        /// Timestamp of when user connected with a macro client for the last time
        /// </summary>
        [DataMember(Name = "lastMacroConnect")]
        public DateTime LastMacroConnect { get; set; }
    }
}