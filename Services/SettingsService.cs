using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Coflnet.Sky.Settings.Client.Api;

namespace Coflnet.Sky.Commands.Shared
{
    public class SettingsService
    {
        private ConnectionMultiplexer con;
        private SettingsApi api;
        public SettingsService(IConfiguration config)
        {
            con = ConnectionMultiplexer.Connect(config["SETTINGS_REDIS_HOST"]);
            api = new SettingsApi(config["SETTINGS_BASE_URL"]);
        }

        public async Task<ChannelMessageQueue> GetAndSubscribe<T>(string userId, string key, Action<T> update, Func<T> defaultGetter = null)
        {
            try
            {
                // subscribe, get then process subscription to not loose any update
                var subTask = con.GetSubscriber().SubscribeAsync(GetSubKey(userId, key));
                var value = await api.SettingsUserIdSettingKeyGetAsync(userId, key);
                T val;
                if (value == null)
                {
                    if (defaultGetter != null)
                        val = defaultGetter();
                    else
                        val = default(T);
                }
                else
                    val = Deserialize<T>(value);
                update(val);
                var sub = await subTask;
                sub.OnMessage(a =>
                {
                    update(Deserialize<T>(a.Message));
                });
                return sub;
            }
            catch (Exception e)
            {
                throw new Exception("Could not subscribe to setting ", e);
            }
        }

        private static string GetSubKey(string userId, string key)
        {
            return userId + key;
        }

        public async Task UpdateSetting<T>(string userId, string key, T data)
        {
            await api.SettingsUserIdSettingKeyPostAsync(userId, key, JsonConvert.SerializeObject(data));
        }

        private static T Deserialize<T>(string a)
        {
            if (a.StartsWith("\""))
                a = JsonConvert.DeserializeObject<string>(a);
            return JsonConvert.DeserializeObject<T>(a);
        }
    }
}
