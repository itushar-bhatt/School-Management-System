using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IAdmissionService
    {
        Task<(bool Success, string Message, object Result)> AdmitStudentAsync(AdmissionRequest request);
    }
}