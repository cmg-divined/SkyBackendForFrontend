using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using hypixel;

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
                if (item.FieldType.IsPrimitive || item.FieldType == typeof(string))
                    options.Add(prefix + (item.GetCustomAttributes(typeof(DataMemberAttribute), true).First() as DataMemberAttribute).Name, item.Name);
            }
            options.Remove("changer");
            options.Remove("showavgSellTime");
        }

        public string[] Options()
        {
            return options.Keys.ToArray();
        }
        public async Task Update(IFlipConnection con, string key, string value)
        {
            if (!options.TryGetValue(key, out string realKey))
                throw new CoflnetException("invalid_setting", "the passed setting doesn't exist");
            if (key.StartsWith("show"))
            {
                var field = con.Settings.Visibility?.GetType().GetField(realKey);
                var typedValue = Convert.ChangeType(value, field.FieldType);
                field.SetValue(con.Settings.Visibility, typedValue);
            }
            else if (key.StartsWith("mod"))
            {
                var field = con.Settings.ModSettings?.GetType().GetField(realKey);
                var typedValue = Convert.ChangeType(value, field.FieldType);
                field.SetValue(con.Settings.ModSettings, typedValue);
            }
            else
            {
                var field = con.Settings.GetType().GetField(realKey);
                var typedValue = Convert.ChangeType(value, field.FieldType);
                Console.WriteLine(JsonConvert.SerializeObject(typedValue));
                field.SetValue(con.Settings, typedValue);
            }
        }

    }
}
