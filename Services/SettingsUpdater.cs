using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Coflnet.Sky.Core;
using System.Reflection;

namespace Coflnet.Sky.Commands.Shared
{
    /// <summary>
    /// Mapps key value air of settings to object
    /// </summary>
    public class SettingsUpdater
    {
        private Dictionary<string, SettingDoc> options = new Dictionary<string, SettingDoc>(StringComparer.OrdinalIgnoreCase);

        public class SettingDoc
        {
            public string RealName;
            public string Info;
            public bool Hide;
            public string ShortHand;

            public string Prefix { get; set; }
        }

        public SettingsUpdater()
        {
            AddSettings(typeof(FlipSettings).GetFields(), "");
            AddSettings(typeof(ModSettings).GetFields(), "mod");
            AddSettings(typeof(VisibilitySettings).GetFields(), "show");
        }

        private void AddSettings(System.Reflection.FieldInfo[] fields, string prefix = "")
        {
            foreach (var item in fields)
            {
                if (item.FieldType.IsPrimitive || item.FieldType == typeof(string) || item.FieldType.IsEnum)
                {
                    var commandSlug = (item.GetCustomAttributes(typeof(DataMemberAttribute), true).FirstOrDefault() as DataMemberAttribute)?.Name;
                    if(commandSlug == null)
                        commandSlug = item.Name;
                    var doc = (item.GetCustomAttributes(typeof(SettingsDocAttribute), true).FirstOrDefault() as SettingsDocAttribute);
                    SettingDoc desc = GetDesc(item, doc, prefix);
                    options.Add(prefix + commandSlug, GetDesc(item, doc, prefix));
                    if (doc?.ShortHand != null)
                        options.Add(doc?.ShortHand, GetDesc(item, doc, prefix, true));
                }

            }
        }

        private SettingDoc GetDesc(FieldInfo item, SettingsDocAttribute doc, string prefix, bool isShortHand = false)
        {
            return new SettingDoc()
            {
                RealName = item.Name,
                Prefix = prefix,
                Info = doc?.Description,
                Hide = (doc?.Hide ?? false) || isShortHand,
                ShortHand = doc?.ShortHand
            };
        }

        public string[] Options()
        {
            return options.Keys.ToArray();
        }

        public IEnumerable<KeyValuePair<string,SettingDoc>> ModOptions => options;

        public async Task<object> Update(IFlipConnection con, string key, string value)
        {
            if (key == "blacklist")
                con.Settings.BlackList = JsonConvert.DeserializeObject<List<ListEntry>>(value);
            else if (key == "whitelist")
                con.Settings.WhiteList = JsonConvert.DeserializeObject<List<ListEntry>>(value);
            else if (key == "filter")
                con.Settings.Filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

            else if (!options.TryGetValue(key, out SettingDoc doc))
            {
                var closest = options.Keys.OrderBy(k => Fastenshtein.Levenshtein.Distance(k.ToLower(), key.ToLower())).First();
                throw new UnkownSettingException(key, closest);
            }
            else if (doc.Prefix == "show")
            {
                return UpdateValueOnObject(value, doc.RealName, con.Settings.Visibility);
            }
            else if (doc.Prefix == "mod")
            {
                return UpdateValueOnObject(value, doc.RealName, con.Settings.ModSettings);
            }
            else
            {
                UpdateValueOnObject(value, doc.RealName, con.Settings);
            }
            return value;
        }

        
        public SettingDoc GetDocFor(string key)
        {
            options.TryGetValue(key, out SettingDoc doc);
            return doc;
        }

        public class UnkownSettingException : CoflnetException
        {
            public string Passed;
            public string Closest;

            public UnkownSettingException(string passed, string closest) : base("invalid_setting", $"the setting {passed} doesn't exist, most similar is {closest}")
            {
                Passed = passed;
                Closest = closest;
            }
        }

        private static object UpdateValueOnObject(string value, string realKey, object obj)
        {
            var field = obj.GetType().GetField(realKey);
            object newValue;
            // if no value is provided and its a bool toggle it
            if (string.IsNullOrEmpty(value) && field.FieldType == typeof(bool))
            {
                newValue = !(bool)field.GetValue(obj);
            }
            else if (field.FieldType.IsEnum)
                newValue = Enum.Parse(field.FieldType, value, true);
            else if(field.FieldType.IsPrimitive && field.FieldType != typeof(bool))
                newValue = Convert.ChangeType(NumberParser.Double(value), field.FieldType);
            else
                newValue = Convert.ChangeType(value, field.FieldType);

            field.SetValue(obj, newValue);

            return newValue;
        }
    }
}
