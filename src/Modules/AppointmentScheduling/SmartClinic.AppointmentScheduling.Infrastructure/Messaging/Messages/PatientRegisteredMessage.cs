using System;

namespace SmartClinic.AppointmentScheduling.Infrastructure.Messaging.Messages
{
    // Message contract from Patient module via RabbitMQ
    public sealed class PatientRegisteredMessage
    {
        public Guid PatientId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
        public DateTime? PreferredAppointmentDate { get; init; }
    }
}
