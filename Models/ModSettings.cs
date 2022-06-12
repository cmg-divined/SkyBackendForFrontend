using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class ModSettings
    {
        /// <summary>
        /// Display only the profit instead of cost and median
        /// </summary>
        [DataMember(Name = "justProfit")]
        [SettingsDoc("Display just the profit")]
        public bool DisplayJustProfit;
        /// <summary>
        /// Play a sound when a flip message is sent
        /// </summary>
        [DataMember(Name = "soundOnFlip")]
        [SettingsDoc("Play a sound when a flip is received")]
        public bool PlaySoundOnFlip;
        /// <summary>
        /// Use M and k to shorten larger numbers
        /// </summary>
        [DataMember(Name = "shortNumbers")]
        [SettingsDoc("Use M and k to shorten numbers", "sn")]
        public bool ShortNumbers;
        /// <summary>
        /// Block "flips in 10 seconds" from appearing
        /// </summary>
        [DataMember(Name = "blockTenSecMsg")]
        [SettingsDoc("Hide the flips in 10 seconds message")]
        public bool BlockTenSecondsMsg;
        [DataMember(Name = "format")]
        [SettingsDoc("Custom flip message format")]
        public string Format;
        [DataMember(Name = "chat")]
        [SettingsDoc("Is the chat enabled")]
        public bool Chat;
        /// <summary>
        /// Should a countdown be displayed till the update
        /// </summary>
        /// <value></value>
        [DataMember(Name = "countdown")]
        [SettingsDoc("Show the timer")]
        public bool DisplayTimer;
        [DataMember(Name = "hideNoBestFlip")]
        [SettingsDoc("Hides the message from the hotkey")]
        public bool HideNoBestFlip;
        [DataMember(Name = "timerX")]
        [SettingsDoc("<---> position in percent")]
        public int TimerX;
        [DataMember(Name = "timerY")]
        [SettingsDoc("up/down position in percent")]
        public int TimerY;
        [DataMember(Name = "timerSeconds")]
        [SettingsDoc("how many seconds before the update the timer should be shown")]
        public int TimerSeconds;
        [DataMember(Name = "timerScale")]
        [SettingsDoc("What scale the timer should be displayed with")]
        public float TimerScale;
        [DataMember(Name = "timerPrefix")]
        [SettingsDoc("Custom text to put in front of the timer")]
        public string TimerPrefix;
        [DataMember(Name = "timerPrecision")]
        [SettingsDoc("How many digits the timer should target (3)")]
        public int TimerPrecision;
        [DataMember(Name = "blockedMsg")]
        [SettingsDoc("How many minutes to have pass before showing the x amounts of flips blocked message again")]
        public sbyte MinutesBetweenBlocked;

        [DataMember(Name = "noAdjustToPurse")]
        [SettingsDoc("Stop your max cost from being auto-adjusted to your purse", "ap")]
        public bool NoAdjustToPurse;
        [DataMember(Name = "noBedDelay")]
        [SettingsDoc("Don't delay bed flips, send them imediately instead", "nbd")]
        public bool NoBedDelay;
    }
}