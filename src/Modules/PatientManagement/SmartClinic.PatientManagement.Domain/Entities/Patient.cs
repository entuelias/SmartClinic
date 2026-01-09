using System;
using System.Collections.Generic;
using SmartClinic.PatientManagement.Domain.ValueObjects;
using SmartClinic.PatientManagement.Domain.Events;
using SmartClinic.BuildingBlocks.Domain;

namespace SmartClinic.PatientManagement.Domain.Entities
{
    public class Patient
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public Guid Id { get; private set; }
        public string FullName { get; private set; } = string.Empty;
        public Email Email { get; private set; } = null!;
        public DateTime DateOfBirth { get; private set; }

        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private Patient() { }

        public static Patient Register(Guid id, string fullName, Email email, DateTime dateOfBirth)
        {
            if (id == Guid.Empty) throw new ArgumentException("Id must be provided", nameof(id));
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("FullName is required", nameof(fullName));

            var patient = new Patient
            {
                Id = id,
                FullName = fullName.Trim(),
                Email = email ?? throw new ArgumentNullException(nameof(email)),
                DateOfBirth = dateOfBirth
            };

            var @event = new PatientRegistered(patient.Id, patient.FullName, patient.Email.Value, patient.DateOfBirth);
            patient._domainEvents.Add(@event);

            return patient;
        }

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}
