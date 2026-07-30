using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<(bool Success, string Message, string UserId)> CreateUserAsync(string username, string email, string fullName, string password, string role);
        Task<bool> AddToRoleAsync(string userId, string role);
        Task<bool> RoleExistsAsync(string role);
        Task CreateRoleAsync(string role);
    }
}