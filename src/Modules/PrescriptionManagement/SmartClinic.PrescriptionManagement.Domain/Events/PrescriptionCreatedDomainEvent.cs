using System;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.PrescriptionManagement.Domain.Events
{
    /// <summary>
    /// Domain event raised when a prescription is created.
    /// </summary>
    public class PrescriptionCreated : DomainEvent
    {
        public Guid PrescriptionId { get; set; }

        public Guid AppointmentId { get; set; }

        public PrescriptionCreated()
        {
        }

        public PrescriptionCreated(Guid prescriptionId, Guid appointmentId)
        {
            PrescriptionId = prescriptionId;
            AppointmentId = appointmentId;
        }
    }
}
