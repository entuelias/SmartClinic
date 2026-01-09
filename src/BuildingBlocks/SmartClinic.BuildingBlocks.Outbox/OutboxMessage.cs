using System;
using System.Text.Json;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.BuildingBlocks.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime OccurredOn { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public string? Error { get; set; }

        public static OutboxMessage FromDomainEvent(IDomainEvent domainEvent)
        {
            return new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                OccurredOn = domainEvent.OccurredOn,
                ProcessedOn = null
            };
        }
    }
}
