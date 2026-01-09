using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartClinic.PrescriptionManagement.Domain.Aggregates;
using SmartClinic.PrescriptionManagement.Domain.Repositories;

namespace SmartClinic.PrescriptionManagement.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core implementation of IPrescriptionRepository.
    /// Provides persistence operations for Prescription aggregates.
    /// </summary>
    public class PrescriptionRepository : IPrescriptionRepository
    {
        private readonly PrescriptionDbContext _context;

        public PrescriptionRepository(PrescriptionDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Adds a new prescription to the repository.
        /// Domain events will be automatically persisted to OutboxMessages via DbContext SaveChangesAsync.
        /// </summary>
        public async Task AddAsync(Prescription prescription)
        {
            if (prescription == null)
            {
                throw new ArgumentNullException(nameof(prescription));
            }

            await _context.Prescriptions.AddAsync(prescription);
            // Save changes to persist aggregate and domain events (via outbox) atomically
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a prescription by its unique identifier.
        /// </summary>
        public async Task<Prescription?> GetByIdAsync(Guid id)
        {
            return await _context.Prescriptions
                .Include(p => p.Medications)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
