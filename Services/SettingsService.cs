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

        public async Task<ChannelMessageQueue> GetAndSubscribe<T>(string userId, string key, Action<T> update)
        {
            // subscribe, get then process subscription to not loose any update
            var subTask = con.GetSubscriber().SubscribeAsync(key);
            var value = await api.SettingsUserIdSettingKeyGetAsync(userId, key);
            update(Deserialize<T>(value));
            var sub = await subTask;
            sub.OnMessage(a =>
            {
                update(Deserialize<T>(a.Message));
            });
            return sub;
        }

        public async Task UpdateSetting<T>(string userId, string key, T data)
        {
            await api.SettingsUserIdSettingKeyPostAsync(userId, key, JsonConvert.SerializeObject(data));
        }

        private static T Deserialize<T>(string a)
        {
            return JsonConvert.DeserializeObject<T>(a);
        }
    }
}
