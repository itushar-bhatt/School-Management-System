using System.Threading.Tasks;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface IAdmissionSequenceRepository
    {
        Task<AdmissionSequence> GetBySessionAsync(string academicSession);
        Task AddAsync(AdmissionSequence sequence);
        Task UpdateAsync(AdmissionSequence sequence);
    }
}