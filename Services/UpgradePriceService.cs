using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coflnet.Sky.Api.Client.Api;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coflnet.Sky.Commands.Shared
{
    public class UpgradePriceService : BackgroundService
    {
        private Api.Client.Api.IPricesApi pricesApi;
        private ILogger<UpgradePriceService> logger;
        private Dictionary<string, double> Prices = new Dictionary<string, double>();

        private List<string> ItemsToGet = new() { "RECOMBOBULATOR_3000" };

        public UpgradePriceService(IPricesApi pricesApi, ILogger<UpgradePriceService> logger)
        {
            this.pricesApi = pricesApi;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var item in ItemsToGet)
                    {
                        var price = await pricesApi.ApiItemPriceItemTagCurrentGetAsync(item, 1);
                        if (price == null)
                        {
                            Prices[item] = 1_010_101;
                            logger.LogError("could not get price for " + item);
                        }
                        else
                            Prices[item] = price.Buy;
                    }
                    await Task.Delay(TimeSpan.FromMinutes(30));
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (System.Exception e)
                {
                    logger.LogError(e, "retrieving prices");
                    await Task.Delay(50000);
                }
            }
        }

        public double GetPrice(string itemId)
        {
            return Prices.GetValueOrDefault(itemId, 1);
        }
    }
}