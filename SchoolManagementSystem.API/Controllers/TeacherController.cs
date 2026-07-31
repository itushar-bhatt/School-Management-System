using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;
using SchoolManagementSystem.Domain.Interfaces;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly IAdmissionService _admissionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IParentRepository _parentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentParentRepository _studentParentRepository;

        public TeacherController(
            IAdmissionService admissionService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IParentRepository parentRepository,
            IStudentRepository studentRepository,
            IStudentParentRepository studentParentRepository)
        {
            _admissionService = admissionService;
            _userManager = userManager;
            _roleManager = roleManager;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _studentParentRepository = studentParentRepository;
        }

        [HttpGet("dashboard")]
        public IActionResult GetTeacherDashboard()
        {
            return Ok(new
            {
                message = "Hi Teacher!",
                username = User.Identity.Name,
                role = "Teacher",
                features = new[]
                {
                    "View class schedules",
                    "Manage student information",
                    "Mark attendance",
                    "Enter grades and marks",
                    "Create assignments and tests",
                    "Admit new students with parents",
                    "View students and parents"
                }
            });
        }

        [HttpPost("admit-student")]
        public async Task<IActionResult> AdmitStudent([FromBody] AdmissionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var (success, message, result) = await _admissionService.AdmitStudentAsync(request);

            if (success)
            {
                return Ok(new { message, result });
            }

            return BadRequest(new { message });
        }

        [HttpPost("users/parent")]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Check if parent with this phone already exists
            var existingParent = await _parentRepository.GetByPhoneAsync(model.Phone);
            if (existingParent != null)
            {
                return BadRequest(new { message = "Parent with this phone number already exists" });
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Parent role
                if (!await _roleManager.RoleExistsAsync("Parent"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Parent"));
                }
                await _userManager.AddToRoleAsync(user, "Parent");

                // Create Parent profile
                var parent = new Domain.Entities.Parent
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    Phone = model.Phone,
                    Address = model.Address,
                    Occupation = model.Occupation,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _parentRepository.AddAsync(parent);

                return Ok(new { 
                    message = $"Parent '{model.Username}' created successfully",
                    parent = new {
                        parent.Id,
                        parent.UserId,
                        parent.Phone,
                        user.UserName,
                        user.FullName
                    }
                });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost("link-student-parent")]
        public async Task<IActionResult> LinkStudentParent([FromBody] LinkStudentParentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Verify student exists in Students table
            var student = await _studentRepository.GetByIdAsync(model.StudentId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Verify parent exists in Parents table
            var parent = await _parentRepository.GetByIdAsync(model.ParentId);
            if (parent == null)
            {
                return NotFound(new { message = "Parent not found" });
            }

            // Check if link already exists
            var linkExists = await _studentParentRepository.ExistsAsync(model.StudentId, model.ParentId);
            if (linkExists)
            {
                return BadRequest(new { message = "Student is already linked with this parent" });
            }

            // Create the link
            var studentParent = new Domain.Entities.StudentParent
            {
                Id = Guid.NewGuid().ToString(),
                StudentId = model.StudentId,
                ParentId = model.ParentId
            };

            await _studentParentRepository.AddAsync(studentParent);

            // Get user details for response
            var studentUser = await _userManager.FindByIdAsync(student.UserId);
            var parentUser = await _userManager.FindByIdAsync(parent.UserId);

            return Ok(new { 
                message = "Student linked with parent successfully",
                student = new { 
                    student.Id, 
                    student.AdmissionNo, 
                    student.Class, 
                    student.Section,
                    UserName = studentUser?.UserName,
                    FullName = studentUser?.FullName
                },
                parent = new { 
                    parent.Id, 
                    parent.Phone,
                    UserName = parentUser?.UserName,
                    FullName = parentUser?.FullName
                }
            });
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] string? className, [FromQuery] string? section)
        {
            // Get students from Students table (not AspNetUsers)
            IEnumerable<Domain.Entities.Student> students;

            if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(section))
            {
                // Search by both class and section
                students = await _studentRepository.GetByClassAndSectionAsync(className, section);
            }
            else if (!string.IsNullOrEmpty(className))
            {
                // Search by class only
                var allStudents = await _studentRepository.GetAllAsync();
                students = allStudents.Where(s => s.Class == className);
            }
            else if (!string.IsNullOrEmpty(section))
            {
                // Search by section only
                var allStudents = await _studentRepository.GetAllAsync();
                students = allStudents.Where(s => s.Section == section);
            }
            else
            {
                // Get all students
                students = await _studentRepository.GetAllAsync();
            }

            // Join with UserManager to get user details
            var result = new List<object>();
            foreach (var student in students)
            {
                var user = await _userManager.FindByIdAsync(student.UserId);
                result.Add(new
                {
                    id = student.Id,
                    userId = student.UserId,
                    admissionNo = student.AdmissionNo,
                    class_ = student.Class,
                    section = student.Section,
                    admissionDate = student.AdmissionDate,
                    isActive = student.IsActive,
                    username = user?.UserName,
                    fullName = user?.FullName,
                    email = user?.Email
                });
            }

            return Ok(result);
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetParents()
        {
            // Get all parents from Parents table
            var parents = await _parentRepository.GetAllAsync();

            // Join with UserManager to get user details
            var result = new List<object>();
            foreach (var parent in parents)
            {
                var user = await _userManager.FindByIdAsync(parent.UserId);
                result.Add(new
                {
                    id = parent.Id,
                    userId = parent.UserId,
                    phone = parent.Phone,
                    address = parent.Address,
                    occupation = parent.Occupation,
                    username = user?.UserName,
                    fullName = user?.FullName,
                    email = user?.Email
                });
            }

            return Ok(result);
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            // Get all students and extract unique classes
            var students = await _studentRepository.GetAllAsync();
            var classes = students
                .Where(s => !string.IsNullOrEmpty(s.Class))
                .Select(s => s.Class)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(classes);
        }

        [HttpGet("classes/{className}/sections")]
        public async Task<IActionResult> GetSections(string className)
        {
            // Get all students and extract unique sections for the specified class
            var students = await _studentRepository.GetAllAsync();
            var sections = students
                .Where(s => s.Class == className && !string.IsNullOrEmpty(s.Section))
                .Select(s => s.Section)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            return Ok(sections);
        }
    }
}