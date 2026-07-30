using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Success, string UserName, string FullName)> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, string.Empty, string.Empty);
            }

            return (true, user.UserName ?? string.Empty, user.FullName ?? string.Empty);
        }
    }
}