using System;
using StackExchange.Redis;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Commands.Shared
{
    public class SelfUpdatingValue<T> : IDisposable
    {
        public T Value { get; private set; }
        public event Action<T> OnChange;
        public event Action<T> AfterChange;

        ChannelMessageQueue subTask;

        private SelfUpdatingValue()
        {
            
        }

        public static async Task<SelfUpdatingValue<T>> Create(string userId, string key, Func<T> defaultGetter = null)
        {
            var instance = new SelfUpdatingValue<T>();
            var settings = DiHandler.ServiceProvider.GetRequiredService<SettingsService>();
            instance.subTask = await settings.GetAndSubscribe<T>(userId, key, v =>
            {
                instance.OnChange?.Invoke(v);
                instance.Value = v;
                instance.AfterChange?.Invoke(v);
            },defaultGetter);
            return instance;
        }

        public void Dispose()
        {
            subTask.Unsubscribe();
        }

        public static implicit operator T(SelfUpdatingValue<T> val) => val == null ? default(T) : val.Value;
    }
}
