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
    }
}