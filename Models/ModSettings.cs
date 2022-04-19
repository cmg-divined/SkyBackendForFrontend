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
        public bool DisplayJustProfit;
        /// <summary>
        /// Play a sound when a flip message is sent
        /// </summary>
        [DataMember(Name = "soundOnFlip")]
        public bool PlaySoundOnFlip;
        /// <summary>
        /// Use M and k to shorten larger numbers
        /// </summary>
        [DataMember(Name = "shortNumbers")]
        public bool ShortNumbers;
        /// <summary>
        /// Block "flips in 10 seconds" from appearing
        /// </summary>
        [DataMember(Name = "blockTenSecMsg")]
        public bool BlockTenSecondsMsg;
        [DataMember(Name = "format")]
        public string Format;
        [DataMember(Name = "chat")]
        public bool Chat;
        /// <summary>
        /// Should a countdown be displayed till the update
        /// </summary>
        /// <value></value>
        [DataMember(Name = "countdown")]
        public bool DisplayTimer;
        [DataMember(Name = "hideNoBestFlip")]
        public bool HideNoBestFlip;
        [DataMember(Name = "timerX")]
        public int TimerX;
        [DataMember(Name = "timerY")]
        public int TimerY;
        [DataMember(Name = "timerSeconds")]
        public int TimerSeconds;
        [DataMember(Name = "timerScale")]
        public float TimerScale;
        [DataMember(Name = "timerPrefix")]
        public string TimerPrefix;
    }
}