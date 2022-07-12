using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Coflnet.Sky;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands.Shared
{
    public class FlipConWrapper
    {
        public IFlipConnection Connection;

        private Channel<LowPricedAuction> LowPriced = Channel.CreateBounded<LowPricedAuction>(
                new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.DropWrite });

        private CancellationTokenSource cancellationTokenSource = null;
        private bool stopWrites;

        public int ChannelCount => LowPriced.Reader.Count;

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

            for (int i = 0; i < count; i++)
            {
                _ = Task.Run(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            var flip = await LowPriced.Reader.ReadAsync(stoppingToken).ConfigureAwait(false);
                            if (LowPriced.Reader.Count > 90)
                            {
                                Connection.Log("amany flips waiting " + LowPriced.Reader.Count, Microsoft.Extensions.Logging.LogLevel.Error);
                                flip.AdditionalProps?.TryAdd("long wait", LowPriced.Reader.Count.ToString());
                            }
                            //await limiter.WaitAsync();
                            await Connection.SendFlip(flip).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                        catch (Exception e)
                        {
                            Connection.Log(e.ToString(), Microsoft.Extensions.Logging.LogLevel.Error);
                            dev.Logger.Instance.Error(e, "seding flip to " + Connection.UserId);
                        }
                    }
                }).ConfigureAwait(false);
            }


        }

        public bool AddLowPriced(LowPricedAuction lp)
        {
            if(stopWrites)
                return false;
            var copy = new LowPricedAuction()
            {
                AdditionalProps = lp.AdditionalProps == null ? new Dictionary<string, string>() : new Dictionary<string, string>(lp.AdditionalProps),
                Auction = lp.Auction,
                DailyVolume = lp.DailyVolume,
                Finder = lp.Finder,
                TargetPrice = lp.TargetPrice
            };
            if (Connection?.Settings?.FastMode ?? false)
                try
                {
                    return Connection.SendFlip(copy).Wait(10);
                }
                catch (Exception e)
                {
                    dev.Logger.Instance.Error(e, "fast send ");
                }
            
            return LowPriced.Writer.TryWrite(copy);
        }

        public Task<bool> SendFlip(FlipInstance flip)
        {
            return Connection.SendFlip(flip);
        }

        public void Stop()
        {
            stopWrites = true;
            cancellationTokenSource?.Cancel();
            LowPriced.Writer.TryComplete();
            Connection.Log("canceled by " + Environment.StackTrace);
        }
    }
}
