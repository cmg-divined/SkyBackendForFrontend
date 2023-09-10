using System;
using System.Security.Cryptography;
using System.Text;
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
            var sha = SHA256.Create();
            producer = kafkaCreator.BuildProducer<string, UpdateMessage>(true, b => b.SetDefaultPartitioner((topic, pcount, key, isNull) =>
            {
                if (isNull || key.Length < 3)
                    return Random.Shared.Next() % pcount;
                byte[] encoded = sha.ComputeHash(key.ToArray());
                int partition = BitConverter.ToUInt16(encoded, 0) % pcount;
                return partition;
            }));

            _ = kafkaCreator.CreateTopicIfNotExist(config["TOPICS:STATE_UPDATE"], 16);
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
