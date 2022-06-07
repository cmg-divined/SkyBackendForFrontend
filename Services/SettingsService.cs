using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Coflnet.Sky.Settings.Client.Api;
using Microsoft.Extensions.Logging;

namespace Coflnet.Sky.Commands.Shared
{
    public class SettingsService
    {
        private ConnectionMultiplexer con;
        private SettingsApi api;
        public SettingsService(IConfiguration config, ILogger<SettingsService> logger)
        {
            var redis = config["SETTINGS_REDIS_HOST"];
            if (redis == null)
                logger.LogWarning("SETTINGS_REDIS_HOST is not set, settings updates will not be received");
            else
                con = ConnectionMultiplexer.Connect(redis);
            api = new SettingsApi(config["SETTINGS_BASE_URL"]);
        }

        public async Task<ChannelMessageQueue> GetAndSubscribe<T>(string userId, string key, Action<T> update, Func<T> defaultGetter = null)
        {
            try
            {
                // subscribe, get then process subscription to not loose any update
                var subTask = con?.GetSubscriber().SubscribeAsync(GetSubKey(userId, key));
                T val = await GetCurrentValue(userId, key, defaultGetter);
                update(val);

                if (con == null)
                    return null;

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

        public async Task<T> GetCurrentValue<T>(string userId, string key, Func<T> defaultGetter)
        {
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
            return val;
        }

        private static string GetSubKey(string userId, string key)
        {
            return userId + key;
        }

        public async Task UpdateSetting<T>(string userId, string key, T data)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await api.SettingsUserIdSettingKeyPostAsync(userId, key, JsonConvert.SerializeObject(data));
                    return;
                }
                catch (System.Exception)
                {
                    await Task.Delay(20 * i);
                    if(i == 2)
                        throw;
                }
            }
        }

        private static T Deserialize<T>(string a)
        {
            if (a.StartsWith("\""))
                a = JsonConvert.DeserializeObject<string>(a);
            return JsonConvert.DeserializeObject<T>(a);
        }
    }
}
