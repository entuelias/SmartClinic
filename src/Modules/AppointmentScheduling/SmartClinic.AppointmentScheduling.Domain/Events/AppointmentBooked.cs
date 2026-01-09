using System;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.AppointmentScheduling.Domain.Events
{
    public sealed class AppointmentBooked : DomainEvent
    {
        public Guid AppointmentId { get; }
        public Guid PatientId { get; }
        public DateTime AppointmentDate { get; }

        public AppointmentBooked(Guid appointmentId, Guid patientId, DateTime appointmentDate)
        {
            AppointmentId = appointmentId;
            PatientId = patientId;
            AppointmentDate = appointmentDate;
        }
    }
}