using System.Threading.Tasks;
using SmartClinic.PatientManagement.Domain.Entities;

namespace SmartClinic.PatientManagement.Domain.Repositories
{
    public interface IPatientRepository
    {
        Task AddAsync(Patient patient);
    }
}
