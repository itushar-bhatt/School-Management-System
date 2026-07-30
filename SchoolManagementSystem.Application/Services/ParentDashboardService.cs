using System;
using System.Linq;
using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class ParentDashboardService : IParentDashboardService
    {
        private readonly IUserService _userService;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentParentRepository _studentParentRepository;
        private readonly IStudentRepository _studentRepository;

        public ParentDashboardService(
            IUserService userService,
            IParentRepository parentRepository,
            IStudentParentRepository studentParentRepository,
            IStudentRepository studentRepository)
        {
            _userService = userService;
            _parentRepository = parentRepository;
            _studentParentRepository = studentParentRepository;
            _studentRepository = studentRepository;
        }

        public async Task<ParentDashboardResponse> GetDashboardAsync(string userId)
        {
            // Get parent profile
            var parent = await _parentRepository.GetByUserIdAsync(userId);
            if (parent == null)
            {
                return new ParentDashboardResponse
                {
                    Message = "Parent profile not found",
                    Username = string.Empty,
                    FullName = string.Empty,
                    Children = new List<StudentInfo>()
                };
            }

            // Get user details
            var (userSuccess, userName, userFullName) = await _userService.GetUserByIdAsync(userId);

            // Get all student-parent links for this parent
            var links = await _studentParentRepository.GetByParentIdAsync(parent.Id);

            // Get children details
            var children = new List<StudentInfo>();
            foreach (var link in links)
            {
                var student = await _studentRepository.GetByIdAsync(link.StudentId);
                if (student != null)
                {
                    var (_, studentUserName, studentFullName) = await _userService.GetUserByIdAsync(student.UserId);
                    children.Add(new StudentInfo
                    {
                        Id = student.Id,
                        AdmissionNo = student.AdmissionNo,
                        FullName = studentFullName,
                        Class = student.Class,
                        Section = student.Section
                    });
                }
            }

            return new ParentDashboardResponse
            {
                Message = "Hello Parent!",
                Username = userName,
                FullName = userFullName,
                Children = children
            };
        }
    }
}