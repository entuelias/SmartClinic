using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartClinic.AppointmentScheduling.Domain.Entities;
using SmartClinic.AppointmentScheduling.Domain.Repositories;

namespace SmartClinic.AppointmentScheduling.Infrastructure.Persistence
{
    public sealed class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentDbContext _context;

        public AppointmentRepository(AppointmentDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Appointment appointment)
        {
            if (appointment is null) throw new ArgumentNullException(nameof(appointment));

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> GetByIdAsync(Guid id)
        {
            return await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
