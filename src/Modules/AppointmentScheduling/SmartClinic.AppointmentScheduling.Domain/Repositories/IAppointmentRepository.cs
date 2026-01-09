using System;
using System.Threading.Tasks;
using SmartClinic.AppointmentScheduling.Domain.Entities;

namespace SmartClinic.AppointmentScheduling.Domain.Repositories
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment);
        Task<Appointment?> GetByIdAsync(Guid id);
    }
}
