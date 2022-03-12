using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Coflnet.Payments.Client.Api;
using System;
using Coflnet.Payments.Client.Client;

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
            services.AddSingleton<SettingsService>();
            services.AddSingleton<GemPriceService>();
            services.AddHostedService<GemPriceService>(di => di.GetRequiredService<GemPriceService>());
            services.AddSingleton<FlipTrackingService>();
            services.AddPaymentSingleton<ProductsApi>(url => new ProductsApi(url));
            services.AddPaymentSingleton<UserApi>(url => new UserApi(url));
            services.AddPaymentSingleton<TopUpApi>(url => new TopUpApi(url));

            _servics = services;
        }

        public static void AddPaymentSingleton<T>(this IServiceCollection services, Func<string, T> creator) where T : class, IApiAccessor
        {
            services.AddSingleton<T>(context =>
            {
                var config = context.GetRequiredService<IConfiguration>();
                return creator("http://" + SimplerConfig.Config.Instance["PAYMENTS_HOST"]);
            });
        }
    }
}