using System;
using System.Collections.Generic;
using MediatR;
using SmartClinic.PrescriptionManagement.Domain.ValueObjects;

namespace SmartClinic.PrescriptionManagement.Application.Commands
{
    /// <summary>
    /// Command to create a new prescription.
    /// </summary>
    public sealed class CreatePrescriptionCommand : IRequest<Guid>
    {
        public Guid AppointmentId { get; init; }
        public List<PrescriptionItemDto> Medications { get; init; } = new();
        public string? Notes { get; init; }
    }

    /// <summary>
    /// DTO for prescription item medication details.
    /// </summary>
    public sealed class PrescriptionItemDto
    {
        public string MedicationName { get; init; } = string.Empty;
        public string Dosage { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}
