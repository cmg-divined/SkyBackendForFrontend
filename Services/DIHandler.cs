using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Coflnet.Sky.Commands.Shared
{
    public static class DiHandler
    {
        private static System.IServiceProvider _serviceProvider;
        public static System.IServiceProvider ServiceProvider
        {
            get 
            {
                if(_serviceProvider == null)
                    _serviceProvider = _servics.BuildServiceProvider();
                return _serviceProvider;
            }
        }

        private static IServiceCollection _servics;
        public static void AddCoflService(this IServiceCollection services)
        {
            services.AddSingleton<PlayerName.Client.Api.PlayerNameApi>(context => {
                var config = context.GetRequiredService<IConfiguration>();
                return new PlayerName.Client.Api.PlayerNameApi(config["PLAYERNAME_HOST"]);
            });

            _servics = services;
        }
    }
}