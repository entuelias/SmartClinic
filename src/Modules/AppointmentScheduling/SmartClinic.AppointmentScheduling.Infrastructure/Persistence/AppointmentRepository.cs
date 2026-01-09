using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using SmartClinic.AppointmentScheduling.Domain.Entities;
using SmartClinic.AppointmentScheduling.Domain.Repositories;

namespace SmartClinic.AppointmentScheduling.Infrastructure.Persistence
{
    // Simple in-memory repository for testing via API; replace with real persistence in production.
    public sealed class AppointmentRepository : IAppointmentRepository
    {
        private readonly ConcurrentDictionary<Guid, Appointment> _store = new();

        public Task AddAsync(Appointment appointment)
        {
            if (appointment is null) throw new ArgumentNullException(nameof(appointment));

            _store[appointment.Id] = appointment;
            return Task.CompletedTask;
        }

        public Task<Appointment?> GetByIdAsync(Guid id)
        {
            _store.TryGetValue(id, out var appointment);
            return Task.FromResult(appointment);
        }
    }
}
