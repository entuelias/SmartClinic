using System;
using System.Collections.Generic;

namespace SmartClinic.PrescriptionManagement.Api.Dtos
{
    /// <summary>
    /// Request DTO for creating a prescription.
    /// </summary>
    public sealed class CreatePrescriptionRequest
    {
        public Guid AppointmentId { get; init; }
        public List<PrescriptionItemRequest> Medications { get; init; } = new();
        public string? Notes { get; init; }
    }

    /// <summary>
    /// Request DTO for a prescription item.
    /// </summary>
    public sealed class PrescriptionItemRequest
    {
        public string MedicationName { get; init; } = string.Empty;
        public string Dosage { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}

