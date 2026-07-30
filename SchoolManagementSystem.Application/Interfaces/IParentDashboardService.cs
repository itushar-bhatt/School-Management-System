using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IParentDashboardService
    {
        Task<ParentDashboardResponse> GetDashboardAsync(string userId);
    }
}