using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Coflnet.Payments.Client.Api;
using System;
using Coflnet.Payments.Client.Client;
using Coflnet.Sky.Items.Client.Api;
using Coflnet.Sky.Referral.Client.Api;
using Coflnet.Sky.Sniper.Client.Api;
using Coflnet.Sky.Crafts.Client.Api;
using Coflnet.Sky.Core;
using Coflnet.Sky.Commands.Shared;

namespace Coflnet.Sky.Commands.Shared
{
    public static class DiHandler
    {
        private static System.IServiceProvider _serviceProvider;
        public static System.IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    _serviceProvider = _servics.BuildServiceProvider();
                return _serviceProvider;
            }
        }

        private static IServiceCollection _servics;
        public static void AddCoflService(this IServiceCollection services)
        {
            services.AddSingleton<PlayerName.Client.Api.PlayerNameApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new PlayerName.Client.Api.PlayerNameApi(config["PLAYERNAME_URL"] ?? "http://" + config["PLAYERNAME_HOST"]);
            });
            services.AddSingleton<Bazaar.Client.Api.BazaarApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                var url = config["BAZAAR_BASE_URL"];
                if (url == null)
                    throw new Exception("config option BAZAAR_BASE_URL is not set");
                return new Bazaar.Client.Api.BazaarApi(url);
            });
            services.AddSingleton<SettingsService>();
            services.AddSingleton<GemPriceService>();
            services.AddHostedService<GemPriceService>(di => di.GetRequiredService<GemPriceService>());
            services.AddSingleton<FlipTrackingService>();
            services.AddPaymentSingleton<ProductsApi>(url => new ProductsApi(url));
            services.AddPaymentSingleton<UserApi>(url => new UserApi(url));
            services.AddPaymentSingleton<TopUpApi>(url => new TopUpApi(url));
            services.AddSingleton<IItemsApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new ItemsApi(config["ITEMS_BASE_URL"]);
            });
            services.AddSingleton<IReferralApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new ReferralApi(config["REFERRAL_BASE_URL"]);
            });
            services.AddSingleton<ISniperApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new SniperApi(config["SNIPER_BASE_URL"]);
            });
            services.AddSingleton<ICraftsApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new CraftsApi("http://" + config["CRAFTS_HOST"]);
            });
            services.AddSingleton<IKatApi>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return new KatApi("http://" + config["CRAFTS_HOST"]);
            });
            services.AddSingleton<PremiumService>();
            services.AddSingleton<EventBrokerClient>();

            _servics = services;
        }

        public static void AddPaymentSingleton<T>(this IServiceCollection services, Func<string, T> creator) where T : class, IApiAccessor
        {
            services.AddSingleton<T>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                var url = SimplerConfig.Config.Instance["PAYMENTS_BASE_URL"];
                if (url == null)
                    url = "http://" + SimplerConfig.Config.Instance["PAYMENTS_HOST"];
                return creator(url);
            });
        }

        public static T GetService<T>(this MessageData di)
        {
            return ServiceProvider.GetService<T>();
        }
    }
}

namespace Coflnet.Sky.Core
{
    public static class DiExtentions
    {
        public static T GetService<T>(this MessageData di)
        {
            return DiHandler.ServiceProvider.GetService<T>();
        }
    }
}