using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IStudentDashboardService
    {
        Task<StudentDashboardResponse> GetDashboardAsync(string userId);
    }
}