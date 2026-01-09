using System;

namespace SmartClinic.BuildingBlocks.Domain
{
    /// <summary>
    /// Base domain event class providing EventId and UTC OccurredOn timestamp.
    /// Keep this in the Shared Kernel / Domain primitives project so all Domain projects
    /// can reference it without pulling in infrastructure.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        protected DomainEvent()
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
        }

        public Guid EventId { get; protected set; }
        public DateTime OccurredOn { get; protected set; }
    }
}
