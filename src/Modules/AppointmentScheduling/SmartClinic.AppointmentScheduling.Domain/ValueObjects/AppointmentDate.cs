using System;

namespace SmartClinic.AppointmentScheduling.Domain.ValueObjects
{
    public sealed class AppointmentDate
    {
        public DateTime Value { get; }

        public AppointmentDate(DateTime value)
        {
            if (value == default)
                throw new ArgumentException("Appointment date is required.", nameof(value));

            if (value <= DateTime.UtcNow)
                throw new ArgumentException("Appointment date must be in the future.", nameof(value));

            Value = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
        }

        public override string ToString() => Value.ToString("O");

        public override bool Equals(object? obj)
        {
            if (obj is AppointmentDate other) return Value.Equals(other.Value);
            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();
    }
}