using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Commands.Shared
{
    public class SelfUpdatingValue<T> : IDisposable
    {
        public T Value { get; private set; }

        ChannelMessageQueue subTask;

        private SelfUpdatingValue()
        {
            
        }

        public static async Task<SelfUpdatingValue<T>> Create(string userId, string key)
        {
            var instance = new SelfUpdatingValue<T>();
            var settings = DiHandler.ServiceProvider.GetRequiredService<SettingsService>();
            instance.subTask = await settings.GetAndSubscribe<T>(userId, key, v =>
            {
                instance.Value = v;
            });
            return instance;
        }

        public void Dispose()
        {
            subTask.Unsubscribe();
        }
    }
}
