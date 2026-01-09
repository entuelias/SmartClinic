using System;
using System.Threading.Tasks;
using SmartClinic.PatientManagement.Domain.Entities;
using SmartClinic.PatientManagement.Domain.Repositories;

namespace SmartClinic.PatientManagement.Infrastructure.Persistence
{
    public class PatientRepository : IPatientRepository
    {
        private readonly PatientDbContext _context;

        public PatientRepository(PatientDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task AddAsync(Patient patient)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }
    }
}
