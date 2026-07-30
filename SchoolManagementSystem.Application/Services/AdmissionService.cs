using System;
using System.Linq;
using System.Threading.Tasks;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Interfaces;

namespace SchoolManagementSystem.Application.Services
{
    public class AdmissionService : IAdmissionService
    {
        private readonly IIdentityService _identityService;
        private readonly IStudentRepository _studentRepository;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentParentRepository _studentParentRepository;

        public AdmissionService(
            IIdentityService identityService,
            IStudentRepository studentRepository,
            IParentRepository parentRepository,
            IStudentParentRepository studentParentRepository)
        {
            _identityService = identityService;
            _studentRepository = studentRepository;
            _parentRepository = parentRepository;
            _studentParentRepository = studentParentRepository;
        }

        public async Task<(bool Success, string Message, object Result)> AdmitStudentAsync(AdmissionRequest request)
        {
            try
            {
                // Step 1: Create Student Identity User
                var (studentSuccess, studentMessage, studentUserId) = await _identityService.CreateUserAsync(
                    request.Student.Username,
                    $"{request.Student.Username}@school.com",
                    request.Student.Name,
                    request.Student.Password,
                    "Student");

                if (!studentSuccess)
                {
                    return (false, $"Failed to create student account: {studentMessage}", null);
                }

                // Step 2: Create Student Profile
                var student = new Student
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = studentUserId,
                    AdmissionNo = request.Student.AdmissionNo,
                    Class = request.Student.Class,
                    Section = request.Student.Section,
                    AdmissionDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _studentRepository.AddAsync(student);

                // Step 3: Search for Parent by Phone
                var existingParent = await _parentRepository.GetByPhoneAsync(request.Parent.Phone);
                Parent parentProfile;
                bool isNewParent = false;

                if (existingParent == null)
                {
                    // Step 4a: Create Parent Identity User (if not exists)
                    var (parentSuccess, parentMessage, parentUserId) = await _identityService.CreateUserAsync(
                        request.Parent.Username,
                        $"{request.Parent.Username}@example.com",
                        request.Parent.Username,
                        request.Parent.Password,
                        "Parent");

                    if (!parentSuccess)
                    {
                        // Rollback: Delete student user if parent creation fails
                        // Note: In production, use a transaction
                        return (false, $"Failed to create parent account: {parentMessage}", null);
                    }

                    // Create Parent Profile
                    parentProfile = new Parent
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = parentUserId,
                        Phone = request.Parent.Phone,
                        Address = request.Parent.Address,
                        Occupation = request.Parent.Occupation,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    await _parentRepository.AddAsync(parentProfile);
                    isNewParent = true;
                }
                else
                {
                    // Use existing parent
                    parentProfile = existingParent;
                }

                // Step 5: Create Student-Parent Link
                var linkExists = await _studentParentRepository.ExistsAsync(student.Id, parentProfile.Id);
                if (linkExists)
                {
                    return (false, "Student is already linked with this parent", null);
                }

                var studentParent = new StudentParent
                {
                    Id = Guid.NewGuid().ToString(),
                    StudentId = student.Id,
                    ParentId = parentProfile.Id
                };

                await _studentParentRepository.AddAsync(studentParent);

                // Step 6: Return success response
                var result = new
                {
                    student = new
                    {
                        student.Id,
                        student.UserId,
                        student.AdmissionNo,
                        student.Class,
                        student.Section,
                        Username = request.Student.Username,
                        FullName = request.Student.Name
                    },
                    parent = new
                    {
                        parentProfile.Id,
                        parentProfile.UserId,
                        parentProfile.Phone,
                        IsNewParent = isNewParent
                    },
                    link = new
                    {
                        studentParent.Id,
                        studentParent.StudentId,
                        studentParent.ParentId
                    }
                };

                return (true, "Student admitted successfully and linked with parent", result);
            }
            catch (Exception ex)
            {
                return (false, $"Admission failed: {ex.Message}", null);
            }
        }
    }
}