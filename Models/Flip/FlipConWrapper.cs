using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Coflnet.Sky;
using Coflnet.Sky.Commands;

namespace hypixel
{
    public class FlipConWrapper
    {
        public IFlipConnection Connection;

        private Channel<LowPricedAuction> LowPriced = Channel.CreateBounded<LowPricedAuction>(100);

        private CancellationTokenSource cancellationTokenSource = null;

        public FlipConWrapper(IFlipConnection connection)
        {
            Connection = connection;
        }

        public async Task Work()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            var stoppingToken = cancellationTokenSource.Token;
            var count = Connection.LatestSettings.Tier switch 
            {
                AccountTier.PREMIUM => 3,
                AccountTier.SUPER_PREMIUM => 6,
                _ => 1
            };
            var limiter = new SemaphoreSlim(count);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                var flip = await LowPriced.Reader.ReadAsync(stoppingToken);
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await limiter.WaitAsync();
                        await Connection.SendFlip(flip);
                    }
                    finally
                    {
                        limiter.Release();
                    }
                });
            }
        }

        public bool AddLowPriced(LowPricedAuction lp)
        {
            return LowPriced.Writer.TryWrite(lp);
        }

        public Task<bool> SendFlip(FlipInstance flip)
        {
            return Connection.SendFlip(flip);
        }

        public void Stop()
        {
            cancellationTokenSource?.Cancel();
        }


    }
}
