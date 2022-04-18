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
        private Dictionary<string, string> options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

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
                    options.Add(prefix + (item.GetCustomAttributes(typeof(DataMemberAttribute), true).First() as DataMemberAttribute).Name, item.Name);
            }
            options.Remove("changer");
            options.Remove("showavgSellTime");
        }

        public string[] Options()
        {
            return options.Keys.ToArray();
        }
        public async Task<object> Update(IFlipConnection con, string key, string value)
        {
            if (key == "blacklist")
                con.Settings.BlackList = JsonConvert.DeserializeObject<List<ListEntry>>(value);
            else if (key == "whitelist")
                con.Settings.WhiteList = JsonConvert.DeserializeObject<List<ListEntry>>(value);
            else if (key == "filter")
                con.Settings.Filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

            else if (!options.TryGetValue(key, out string realKey))
                throw new CoflnetException("invalid_setting", "the passed setting doesn't exist");
            else if (key.StartsWith("show"))
            {
                return UpdateValueOnObject(value, realKey, con.Settings.Visibility);
            }
            else if (key.StartsWith("mod"))
            {
                return UpdateValueOnObject(value, realKey, con.Settings.ModSettings);
            }
            else
            {
                UpdateValueOnObject(value,realKey, con.Settings);
            }
            return value;
        }

        private static object UpdateValueOnObject(string value, string realKey, object obj)
        {
            var field = obj.GetType().GetField(realKey);
            if (value.ToLower().EndsWith('m') && field.FieldType.IsPrimitive)
                value = value.ToLower().Replace("m", "000000");
            if (value.ToLower().EndsWith('k') && field.FieldType.IsPrimitive)
                value = value.ToLower().Replace("k", "000");
            object newValue;
            // if no value is provided and its a bool toggle it
            if (string.IsNullOrEmpty(value) && field.FieldType == typeof(bool))
            {
                newValue = !(bool)field.GetValue(obj);
            }
            else if (field.FieldType.IsEnum)
                newValue = Enum.Parse(field.FieldType, value);
            else
                newValue = Convert.ChangeType(value, field.FieldType);

            field.SetValue(obj, newValue);

            return newValue;
        }
    }
}
