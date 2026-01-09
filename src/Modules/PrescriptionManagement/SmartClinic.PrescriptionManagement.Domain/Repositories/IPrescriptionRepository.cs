using System;
using System.Threading.Tasks;
using SmartClinic.PrescriptionManagement.Domain.Aggregates;

namespace SmartClinic.PrescriptionManagement.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Prescription aggregate root.
    /// </summary>
    public interface IPrescriptionRepository
    {
        /// <summary>
        /// Adds a new prescription to the repository.
        /// </summary>
        Task AddAsync(Prescription prescription);

        /// <summary>
        /// Retrieves a prescription by its unique identifier.
        /// </summary>
        Task<Prescription?> GetByIdAsync(Guid id);
    }
}

