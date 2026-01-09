namespace SmartClinic.PrescriptionManagement.Domain.ValueObjects
{
    /// <summary>
    /// Value object describing a medication entry in a prescription.
    /// </summary>
    public class PrescriptionItem
    {
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
