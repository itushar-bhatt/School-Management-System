using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IUserService
    {
        Task<(bool Success, string UserName, string FullName)> GetUserByIdAsync(string userId);
    }
}