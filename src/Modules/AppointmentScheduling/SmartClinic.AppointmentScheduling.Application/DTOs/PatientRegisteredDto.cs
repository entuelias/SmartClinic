using System;

namespace SmartClinic.AppointmentScheduling.Application.DTOs
{
    public sealed class PatientRegisteredDto
    {
        public Guid PatientId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
    }
}
