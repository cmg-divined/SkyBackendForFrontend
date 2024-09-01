using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Coflnet.Sky.Api.Client.Model;

namespace Coflnet.Sky.Commands.Shared;
public class SettingsDiffer
{
    /// <summary>
    /// Compares two settings objects and returns the differences
    /// </summary>
    /// <param name="oldSettings">The old settings</param>
    /// <param name="newSettings">The new settings</param>
    /// <returns>The differences between the two settings</returns>
    public static SettingsDiff GetDifferences(FlipSettings oldSettings, FlipSettings newSettings)
    {
        var differences = new SettingsDiff();
        // iterate over all properties of settings
        AssignSetCommandsDiff(oldSettings, newSettings, differences);
        AssignSetCommandsDiff(oldSettings.ModSettings, newSettings.ModSettings, differences, "mod");
        AssignSetCommandsDiff(oldSettings.Visibility, newSettings.Visibility, differences, "show");


        CompareList(differences, oldSettings.BlackList, newSettings.BlackList);
        CompareList(differences, oldSettings.WhiteList, newSettings.WhiteList);

        return differences;
    }

    public FlipSettings ApplyDiff(FlipSettings settings, SettingsDiff settingsDiff, bool skipNonfilter = false)
    {
        var updater = new SettingsUpdater();
        var error = new List<(string message, ListEntry affected)>();
        if (!skipNonfilter)
        {
            foreach (var command in settingsDiff.SetCommands)
            {
                var parts = command.Split(' ');
                updater.Update(settings, parts[0], parts[1]);
            }
        }
        foreach (var command in settingsDiff.BlacklistAdded)
        {
            settings.BlackList.Add(command);
        }
        foreach (var command in settingsDiff.BlacklistChanged)
        {
            var previous = settings.BlackList.FirstOrDefault(e => GetKey(e) == command.Key);
            if (previous != null)
            {
                settings.BlackList.Remove(previous);
            }
            else
            {
                error.Add(($"Could not find previous entry to remove", command.Value));
            }
            settings.BlackList.Add(command.Value);
        }
        foreach (var command in settingsDiff.BlacklistRemoved)
        {
            settings.BlackList.Remove(command);
        }
        foreach (var command in settingsDiff.WhitelistAdded)
        {
            settings.WhiteList.Add(command);
        }
        foreach (var command in settingsDiff.WhitelistChanged)
        {
            var previous = settings.WhiteList.FirstOrDefault(e => GetKey(e) == command.Key);
            if (previous != null)
            {
                settings.WhiteList.Remove(previous);
            }
            else
            {
                error.Add(($"Could not find previous entry to remove", command.Value));
            }
            settings.WhiteList.Add(command.Value);
        }
        foreach (var command in settingsDiff.WhitelistRemoved)
        {
            settings.WhiteList.Remove(command);
        }
        return settings;
    }

    private static void CompareList(SettingsDiff differences, List<ListEntry> oldBlacklist, List<ListEntry> newBlacklist)
    {
        if (oldBlacklist == null || newBlacklist == null)
        {
            return;
        }
        var oldLookup = oldBlacklist.ToLookup(e => GetKey(e));
        // iterate over all blacklist entries
        foreach (var entry in newBlacklist)
        {
            var elementKey = GetKey(entry);
            // if the entry is not in the old blacklist
            if (!oldLookup.Contains(elementKey))
            {
                // add the entry to the blacklist added list
                differences.BlacklistAdded.Add(entry);
            }
            else
            {
                // get the old entry
                var oldEntry = oldLookup[elementKey].First();
                // if the entries are not equal
                if (!entry.Equals(oldEntry))
                {
                    // add the entry to the blacklist changed list
                    differences.BlacklistChanged[elementKey] = entry;
                }
            }
        }
        // iterate over all old blacklist entries
        foreach (var entry in oldBlacklist)
        {
            var elementKey = GetKey(entry);
            // if the entry is not in the new blacklist
            if (!newBlacklist.Any(e => GetKey(e) == elementKey))
            {
                // add the entry to the blacklist removed list
                differences.BlacklistRemoved.Add(entry);
            }
        }
    }

    private static string GetKey(ListEntry e)
    {
        // filters used to narrow down an item, not affecting actual actions are part of the id
        var relevantFilters = e.filter?.Where(f =>
            f.Key.Equals("petlevel", System.StringComparison.OrdinalIgnoreCase)
            || f.Key.Equals("tier", System.StringComparison.OrdinalIgnoreCase)
            || f.Key.Equals("Recombobulated", System.StringComparison.OrdinalIgnoreCase)
        ).Select(f => f.Key + "=" + f.Value);
        return e.ItemTag + (e.Tags == null ? string.Empty : string.Join(',', e.Tags))
            + (relevantFilters == null ? string.Empty : string.Join(',', relevantFilters));
    }

    private static void AssignSetCommandsDiff<T>(T oldSettings, T newSettings, SettingsDiff differences, string prefix = "")
    {
        if (oldSettings == null || newSettings == null)
        {
            return;
        }
        foreach (var property in typeof(T).GetFields())
        {
            // if object continue
            if (property.FieldType.IsClass && property.FieldType != typeof(string))
            {
                continue;
            }
            // get the value of the property in the old settings
            var oldValue = property.GetValue(oldSettings);
            // get the value of the property in the new settings
            var newValue = property.GetValue(newSettings);
            var dataMembername = property.GetCustomAttribute<DataMemberAttribute>()?.Name;
            // if the values are not equal
            if (!Equals(oldValue, newValue) && dataMembername != null)
            {
                // add a set command to the differences
                differences.SetCommands.Add($"{prefix + dataMembername} {newValue}");
            }
        }
    }

    public class SettingsDiff
    {
        public List<string> SetCommands { get; set; } = [];
        public List<ListEntry> BlacklistAdded { get; set; } = [];
        public Dictionary<string, ListEntry> BlacklistChanged { get; set; } = [];
        public List<ListEntry> BlacklistRemoved { get; set; } = [];
        public List<ListEntry> WhitelistAdded { get; set; } = [];
        public Dictionary<string, ListEntry> WhitelistChanged { get; set; } = [];
        public List<ListEntry> WhitelistRemoved { get; set; } = [];
    }
}

