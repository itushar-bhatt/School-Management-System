using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;

namespace SchoolManagementSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync();
    }
}