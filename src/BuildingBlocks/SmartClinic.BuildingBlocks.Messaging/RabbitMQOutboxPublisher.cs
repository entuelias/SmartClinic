using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using SmartClinic.BuildingBlocks.Outbox;

namespace SmartClinic.BuildingBlocks.Messaging
{
    public class RabbitMQOutboxPublisher : IOutboxPublisher
    {
        private readonly IConnectionFactory _connectionFactory;
        private const string ExchangeName = "smartclinic.events";

        public RabbitMQOutboxPublisher(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            // RabbitMQ.Client 6.x is synchronous
            // We wrap it in Task.Run if we want to pretend it's async, or just run it synchronously.
            // Since IOutboxPublisher returns Task, we can use Task.Run or just complete synchronously.
            // However, opening connection every time is expensive.
            // But for this task, I'll follow the previous pattern of opening per publish for simplicity/statelessness, 
            // or better, let's keep it simple and safe.
            
            using var connection = _connectionFactory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message.Payload);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = message.Id.ToString();
            properties.Type = message.Type;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: message.Type,
                mandatory: false,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }
    }
}
