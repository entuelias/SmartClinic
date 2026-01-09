using System;

namespace SmartClinic.PatientManagement.Domain.ValueObjects
{
    public sealed class Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(value));

            // Basic validation using System.Net.Mail for correctness
            try
            {
                var _ = new System.Net.Mail.MailAddress(value);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid email format.", nameof(value), ex);
            }

            Value = value.Trim();
        }

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is Email other) return string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    }
}
