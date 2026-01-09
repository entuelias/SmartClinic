using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartClinic.PrescriptionManagement.Application.Commands;
using SmartClinic.PrescriptionManagement.Domain.Aggregates;
using SmartClinic.PrescriptionManagement.Domain.Repositories;
using SmartClinic.PrescriptionManagement.Domain.ValueObjects;

namespace SmartClinic.PrescriptionManagement.Application.Handlers
{
    /// <summary>
    /// Handler for CreatePrescriptionCommand.
    /// Creates a new prescription aggregate and persists it via repository.
    /// </summary>
    public sealed class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, Guid>
    {
        private readonly IPrescriptionRepository _repository;

        public CreatePrescriptionCommandHandler(IPrescriptionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Guid> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            if (request.AppointmentId == Guid.Empty)
            {
                throw new ArgumentException("AppointmentId is required", nameof(request));
            }

            if (request.Medications == null || request.Medications.Count == 0)
            {
                throw new ArgumentException("At least one medication is required", nameof(request));
            }

            // Convert DTOs to domain value objects
            var medications = request.Medications.Select(m => new PrescriptionItem
            {
                MedicationName = m.MedicationName,
                Dosage = m.Dosage,
                Quantity = m.Quantity
            }).ToList();

            // Create prescription aggregate
            var prescriptionId = Guid.NewGuid();
            var prescription = new Prescription(
                id: prescriptionId,
                appointmentId: request.AppointmentId,
                medications: medications,
                notes: request.Notes
            );

            // Persist via repository (infrastructure-agnostic)
            await _repository.AddAsync(prescription);

            return prescriptionId;
        }
    }
}
