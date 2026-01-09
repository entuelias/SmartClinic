using System;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.PatientManagement.Domain.Events
{
    public sealed class PatientRegistered : DomainEvent
    {
        public Guid PatientId { get; }
        public string FullName { get; }
        public string Email { get; }
        public DateTime DateOfBirth { get; }

        public PatientRegistered(Guid patientId, string fullName, string email, DateTime dateOfBirth)
        {
            PatientId = patientId;
            FullName = fullName;
            Email = email;
            DateOfBirth = dateOfBirth;
        }
    }
}
