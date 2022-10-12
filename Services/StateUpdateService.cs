using System;
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

        public StateUpdateService(IConfiguration config, ILogger<StateUpdateService> logger)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config["KAFKA_HOST"],
                LingerMs = 100
            };
            producer = new ProducerBuilder<string, UpdateMessage>(producerConfig).SetValueSerializer(SerializerFactory.GetSerializer<UpdateMessage>()).SetDefaultPartitioner((topic, pcount, key, isNull) =>
            {
                if (isNull)
                    return Random.Shared.Next() % pcount;
                int partition = Math.Abs((int)key[0] << 8 | key[1] ^ key[2]) % pcount;
                return partition;
            }).Build();
            this.logger = logger;
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
