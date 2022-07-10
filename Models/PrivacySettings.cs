using System;
using System.Collections.Generic;

namespace Coflnet.Sky.Commands.Shared
{
    public class PrivacySettings
    {
        public string ChatRegex;
        public bool CollectChat;
        public bool CollectInventory;
        public bool CollectTab;
        public bool CollectScoreboard;
        public bool AllowProxy;
        public bool CollectInvClick;
        public bool CollectChatClicks;
        public bool CollectLobbyChanges;
        public bool CollectEntities;
        /// <summary>
        /// Wherever or not to send item descriptions for extending to the server
        /// </summary>
        public bool ExtendDescriptions;
        /// <summary>
        /// Chat input starting with one of these prefixes is sent to the server
        /// </summary>
        public string[] CommandPrefixes;
        public bool AutoStart;

        public override bool Equals(object obj)
        {
            return obj is PrivacySettings settings &&
                   ChatRegex == settings.ChatRegex &&
                   CollectChat == settings.CollectChat &&
                   CollectInventory == settings.CollectInventory &&
                   CollectTab == settings.CollectTab &&
                   CollectScoreboard == settings.CollectScoreboard &&
                   AllowProxy == settings.AllowProxy &&
                   CollectInvClick == settings.CollectInvClick &&
                   CollectChatClicks == settings.CollectChatClicks &&
                   CollectLobbyChanges == settings.CollectLobbyChanges &&
                   CollectEntities == settings.CollectEntities &&
                   ExtendDescriptions == settings.ExtendDescriptions &&
                   EqualityComparer<string[]>.Default.Equals(CommandPrefixes, settings.CommandPrefixes) &&
                   AutoStart == settings.AutoStart;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(ChatRegex);
            hash.Add(CollectChat);
            hash.Add(CollectInventory);
            hash.Add(CollectTab);
            hash.Add(CollectScoreboard);
            hash.Add(AllowProxy);
            hash.Add(CollectInvClick);
            hash.Add(CollectChatClicks);
            hash.Add(CollectLobbyChanges);
            hash.Add(CollectEntities);
            hash.Add(ExtendDescriptions);
            hash.Add(CommandPrefixes);
            hash.Add(AutoStart);
            return hash.ToHashCode();
        }
    }
}