using System;

namespace SmartClinic.AppointmentScheduling.Api.Requests
{
    public sealed class BookAppointmentRequest
    {
        public Guid PatientId { get; init; }
        public DateTime AppointmentDate { get; init; }
    }
}
