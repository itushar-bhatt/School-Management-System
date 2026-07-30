using System;
using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly IUserService _userService;
        private readonly IStudentRepository _studentRepository;

        public StudentDashboardService(
            IUserService userService,
            IStudentRepository studentRepository)
        {
            _userService = userService;
            _studentRepository = studentRepository;
        }

        public async Task<StudentDashboardResponse> GetDashboardAsync(string userId)
        {
            // Get student profile
            var student = await _studentRepository.GetByUserIdAsync(userId);
            if (student == null)
            {
                return new StudentDashboardResponse
                {
                    Message = "Student profile not found",
                    Username = string.Empty,
                    FullName = string.Empty,
                    AdmissionNo = string.Empty,
                    Class = string.Empty,
                    Section = string.Empty
                };
            }

            // Get user details
            var (_, userName, userFullName) = await _userService.GetUserByIdAsync(userId);

            return new StudentDashboardResponse
            {
                Message = "Hello Student!",
                Username = userName,
                FullName = userFullName,
                AdmissionNo = student.AdmissionNo,
                Class = student.Class,
                Section = student.Section
            };
        }
    }
}