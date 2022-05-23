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
        private string UserId;
        private string Key;

        ChannelMessageQueue subTask;

        private SelfUpdatingValue(string userId, string key)
        {
            this.UserId = userId;
            this.Key = key;
        }

        public static async Task<SelfUpdatingValue<T>> Create(string userId, string key, Func<T> defaultGetter = null)
        {
            var instance = new SelfUpdatingValue<T>(userId, key);
            SettingsService settings = GetService();
            instance.subTask = await settings.GetAndSubscribe<T>(userId, key, v =>
            {
                instance.OnChange?.Invoke(v);
                instance.Value = v;
                instance.AfterChange?.Invoke(v);
            }, defaultGetter);
            return instance;
        }

        private static SettingsService GetService()
        {
            return DiHandler.ServiceProvider.GetRequiredService<SettingsService>();
        }

        public void Dispose()
        {
            subTask.Unsubscribe();
        }

        /// <summary>
        /// Replaces the current value with a new one
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public async Task Update(T newValue)
        {
            await GetService().UpdateSetting(UserId, Key, newValue);
        }

        /// <summary>
        /// Updates the current value
        /// </summary>
        /// <returns></returns>
        public async Task Update()
        {
            await Update(Value);
        }

        public static implicit operator T(SelfUpdatingValue<T> val) => val == null ? default(T) : val.Value;
    }
}
