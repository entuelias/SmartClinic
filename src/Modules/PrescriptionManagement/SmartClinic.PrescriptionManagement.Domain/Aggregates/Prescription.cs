namespace SmartClinic.PrescriptionManagement.Domain.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SmartClinic.PrescriptionManagement.Domain.Events;
    using SmartClinic.PrescriptionManagement.Domain.ValueObjects;

    /// <summary>
    /// Prescription aggregate root.
    /// </summary>
    public class Prescription
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public List<PrescriptionItem> Medications { get; set; } = new();
        public string? Notes { get; set; }

        // Domain events raised by this aggregate.
        private readonly List<PrescriptionCreated> _domainEvents = new();
        public IReadOnlyCollection<PrescriptionCreated> DomainEvents => _domainEvents.AsReadOnly();

        public Prescription()
        {
        }

        public Prescription(Guid id, Guid appointmentId, List<PrescriptionItem> medications, string? notes = null)
        {
            Id = id;
            AppointmentId = appointmentId;
            Medications = medications ?? new List<PrescriptionItem>();
            Notes = notes;

            _domainEvents.Add(new PrescriptionCreated(id, appointmentId));
        }

        /// <summary>
        /// Clears all domain events from this aggregate.
        /// Called by infrastructure after events are persisted to outbox.
        /// </summary>
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
