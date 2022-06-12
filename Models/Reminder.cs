using System;
using System.Runtime.Serialization;

namespace Coflnet.Sky.Commands.Shared
{
    [DataContract]
    public class Reminder
    {
        [DataMember(Name = "text")]
        public string Text;
        [DataMember(Name = "trigger")]
        public DateTime TriggerTime;

        public Reminder(string text, DateTime triggerTime)
        {
            Text = text;
            TriggerTime = triggerTime;
        }

        public override bool Equals(object obj)
        {
            return obj is Reminder mute &&
                   Text == mute.Text;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text);
        }
    }
}