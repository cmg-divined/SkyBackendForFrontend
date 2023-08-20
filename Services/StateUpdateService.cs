using System;
using Coflnet.Kafka;
using Coflnet.Sky.Core;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Coflnet.Sky.Commands.Shared
{
    public interface IStateUpdateService
    {
        void Produce(string playerId, UpdateMessage message);
    }

    public class StateUpdateService : IStateUpdateService
    {
        IConfiguration config;
        ILogger<StateUpdateService> logger;
        IProducer<string, UpdateMessage> producer;

        public StateUpdateService(IConfiguration config, ILogger<StateUpdateService> logger, KafkaCreator kafkaCreator)
        {
            this.logger = logger;
            this.config = config;
            producer = kafkaCreator.BuildProducer<string, UpdateMessage>(true, b => b.SetDefaultPartitioner((topic, pcount, key, isNull) =>
            {
                if (isNull)
                    return Random.Shared.Next() % pcount;
                int partition = Math.Abs((int)key[0] << 8 | key[1] ^ key[2]) % pcount;
                return partition;
            }));

            _ = kafkaCreator.CreateTopicIfNotExist(config["TOPICS:STATE_UPDATE"], 9);
        }

        public void Produce(string playerId, UpdateMessage message)
        {
            message.PlayerId = playerId;
            producer.Produce(config["TOPICS:STATE_UPDATE"], new()
            {
                Key = string.IsNullOrEmpty(playerId) ? null : playerId.Truncate(4).PadRight(4) + message.GetHashCode(),
                Value = message,
                Timestamp = new(DateTime.UtcNow)
            });
        }
    }
}
