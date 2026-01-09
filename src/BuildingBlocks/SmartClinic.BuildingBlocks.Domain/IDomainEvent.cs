using System;

namespace SmartClinic.BuildingBlocks.Domain
{
    /// <summary>
    /// Minimal domain event marker for Shared Kernel / Domain primitives.
    /// Infrastructure-agnostic POCO.
    /// </summary>
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }
}
