using System;
using System.Threading;
using System.Threading.Tasks;
using SmartClinic.PrescriptionManagement.Domain.Aggregates;

namespace SmartClinic.PrescriptionManagement.Application.Repositories
{
    /// <summary>
    /// Persistence abstraction for prescriptions.
    /// </summary>
    public interface IPrescriptionRepository
    {
        Task AddAsync(Prescription prescription, CancellationToken cancellationToken = default);

        Task<Prescription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

