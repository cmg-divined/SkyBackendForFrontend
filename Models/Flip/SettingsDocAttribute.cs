
using System;

namespace Coflnet.Sky.Commands.Shared
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SettingsDocAttribute : Attribute
    {
        public string Description;
        public bool Hide;
        public string ShortHand;

        public SettingsDocAttribute(string description, bool hide = false, string shortHand = null)
        {
            Description = description;
            Hide = hide;
            ShortHand = shortHand;
        }
        public SettingsDocAttribute(string description, string shortHand) : this(description, false, shortHand)
        {
        }
    }


}
