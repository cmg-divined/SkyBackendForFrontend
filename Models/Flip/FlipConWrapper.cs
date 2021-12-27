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
            while (!stoppingToken.IsCancellationRequested)
            {
                var flip = await LowPriced.Reader.ReadAsync(stoppingToken);
                await Connection.SendFlip(flip);
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
