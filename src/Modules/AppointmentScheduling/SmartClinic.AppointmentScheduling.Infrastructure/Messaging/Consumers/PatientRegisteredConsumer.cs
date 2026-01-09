using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartClinic.AppointmentScheduling.Application.Commands;
using SmartClinic.AppointmentScheduling.Application.DTOs;
using SmartClinic.AppointmentScheduling.Infrastructure.Messaging.Messages;

namespace SmartClinic.AppointmentScheduling.Infrastructure.Messaging.Consumers
{
    // Skeleton consumer; wiring and connection configuration excluded by design
    public sealed class PatientRegisteredConsumer
    {
        private readonly IMediator _mediator;

        public PatientRegisteredConsumer(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public Task SubscribeAsync(IConnection connection, string queueName, CancellationToken cancellationToken = default)
        {
            if (connection is null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrWhiteSpace(queueName)) throw new ArgumentException("Queue name required", nameof(queueName));

            var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<PatientRegisteredMessage>(json);
                    if (message is null)
                    {
                        channel.BasicNack(ea.DeliveryTag, false, false);
                        return;
                    }

                    if (message.PreferredAppointmentDate.HasValue)
                    {
                        var dto = new PatientRegisteredDto
                        {
                            PatientId = message.PatientId,
                            FullName = message.FullName,
                            Email = message.Email,
                            DateOfBirth = message.DateOfBirth
                        };

                        var command = BookAppointmentCommand.FromPatientRegistered(dto, message.PreferredAppointmentDate.Value);
                        await _mediator.Send(command, cancellationToken).ConfigureAwait(false);
                    }

                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
