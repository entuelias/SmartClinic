using System;
using System.Collections.Generic;
using SmartClinic.AppointmentScheduling.Domain.Events;
using SmartClinic.AppointmentScheduling.Domain.ValueObjects;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.AppointmentScheduling.Domain.Entities
{
    public class Appointment
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public Guid Id { get; private set; }
        public Guid PatientId { get; private set; }
        public AppointmentDate AppointmentDate { get; private set; } = null!;
        public AppointmentStatus Status { get; private set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private Appointment() { }

        private Appointment(Guid id, Guid patientId, AppointmentDate appointmentDate)
        {
            Id = id;
            PatientId = patientId;
            AppointmentDate = appointmentDate;
            Status = AppointmentStatus.Booked;
        }

        public static Appointment Book(Guid id, Guid patientId, AppointmentDate appointmentDate)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id must be provided", nameof(id));
            if (patientId == Guid.Empty) throw new ArgumentException("PatientId must be provided", nameof(patientId));
            if (appointmentDate is null) throw new ArgumentNullException(nameof(appointmentDate));

            var appointment = new Appointment(id, patientId, appointmentDate);

            var @event = new AppointmentBooked(appointment.Id, appointment.PatientId, appointment.AppointmentDate.Value);
            appointment._domainEvents.Add(@event);

            return appointment;
        }

        public void Reschedule(AppointmentDate newDate)
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot reschedule a cancelled appointment.");

            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot reschedule a completed appointment.");

            AppointmentDate = newDate ?? throw new ArgumentNullException(nameof(newDate));
            Status = AppointmentStatus.Booked;
        }

        public void MarkCompleted()
        {
            if (Status != AppointmentStatus.Booked)
                throw new InvalidOperationException("Only booked appointments can be completed.");

            Status = AppointmentStatus.Completed;
        }

        public void Cancel()
        {
            if (Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot cancel a completed appointment.");

            Status = AppointmentStatus.Cancelled;
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    public enum AppointmentStatus
    {
        Booked = 0,
        Completed = 1,
        Cancelled = 2
    }
}
