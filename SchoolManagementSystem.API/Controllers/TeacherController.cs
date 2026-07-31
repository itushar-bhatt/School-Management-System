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
        private readonly ITeacherClassRepository _teacherClassRepository;

        public TeacherController(
            IAdmissionService admissionService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IParentRepository parentRepository,
            IStudentRepository studentRepository,
            IStudentParentRepository studentParentRepository,
            ITeacherClassRepository teacherClassRepository)
        {
            _admissionService = admissionService;
            _userManager = userManager;
            _roleManager = roleManager;
            _parentRepository = parentRepository;
            _studentRepository = studentRepository;
            _studentParentRepository = studentParentRepository;
            _teacherClassRepository = teacherClassRepository;
        }

        // Helper: Get current teacher's ID
        private string? GetCurrentTeacherId()
        {
            return User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        }

        // Helper: Get current teacher's assigned classes
        private async Task<List<(string Class, string? Section)>> GetAssignedClassesAsync()
        {
            var teacherId = GetCurrentTeacherId();
            if (string.IsNullOrEmpty(teacherId))
                return new List<(string Class, string? Section)>();

            var assignments = await _teacherClassRepository.GetByTeacherIdAsync(teacherId);
            return assignments.Select(a => (a.Class, a.Section)).ToList();
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetTeacherDashboard()
        {
            var assignedClasses = await GetAssignedClassesAsync();
            
            return Ok(new
            {
                message = "Hi Teacher!",
                username = User.Identity.Name,
                role = "Teacher",
                assignedClasses = assignedClasses.Select(c => new { c.Class, c.Section }),
                features = new[]
                {
                    "View class schedules",
                    "Manage student information",
                    "Mark attendance",
                    "Enter grades and marks",
                    "Create assignments and tests",
                    "Admit new students with parents",
                    "View students and parents (only from assigned classes)"
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

            // Check if teacher is assigned to the student's class
            var assignedClasses = await GetAssignedClassesAsync();
            var isAssigned = assignedClasses.Any(c => 
                c.Class == request.Student.Class && 
                (c.Section == null || c.Section == request.Student.Section));

            if (!isAssigned)
            {
                return Forbid();
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

            // Check if teacher is assigned to this student's class
            var assignedClasses = await GetAssignedClassesAsync();
            var isAssigned = assignedClasses.Any(c => 
                c.Class == student.Class && 
                (c.Section == null || c.Section == student.Section));

            if (!isAssigned)
            {
                return Forbid();
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
            // Get teacher's assigned classes
            var assignedClasses = await GetAssignedClassesAsync();
            
            if (!assignedClasses.Any())
            {
                return Ok(new List<object>());
            }

            // Get all students
            var allStudents = await _studentRepository.GetAllAsync();

            // Filter students by teacher's assigned classes
            var filteredStudents = allStudents.Where(s => 
                assignedClasses.Any(c => 
                    c.Class == s.Class && 
                    (c.Section == null || c.Section == s.Section)));

            // Apply additional filters if provided
            if (!string.IsNullOrEmpty(className))
            {
                filteredStudents = filteredStudents.Where(s => s.Class == className);
            }

            if (!string.IsNullOrEmpty(section))
            {
                filteredStudents = filteredStudents.Where(s => s.Section == section);
            }

            // Join with UserManager to get user details
            var result = new List<object>();
            foreach (var student in filteredStudents)
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
            // Get teacher's assigned classes
            var assignedClasses = await GetAssignedClassesAsync();
            
            if (!assignedClasses.Any())
            {
                return Ok(new List<object>());
            }

            // Get all students from teacher's assigned classes
            var allStudents = await _studentRepository.GetAllAsync();
            var assignedStudents = allStudents.Where(s => 
                assignedClasses.Any(c => 
                    c.Class == s.Class && 
                    (c.Section == null || c.Section == s.Section))).ToList();

            // Get all student-parent links for these students
            var parentIds = new HashSet<string>();
            foreach (var student in assignedStudents)
            {
                var links = await _studentParentRepository.GetByStudentIdAsync(student.Id);
                foreach (var link in links)
                {
                    parentIds.Add(link.ParentId);
                }
            }

            // Get parent details
            var result = new List<object>();
            foreach (var parentId in parentIds)
            {
                var parent = await _parentRepository.GetByIdAsync(parentId);
                if (parent != null)
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
            }

            return Ok(result);
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            // Get teacher's assigned classes
            var assignedClasses = await GetAssignedClassesAsync();
            
            // Return unique classes from assignments
            var classes = assignedClasses
                .Select(c => c.Class)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(classes);
        }

        [HttpGet("classes/{className}/sections")]
        public async Task<IActionResult> GetSections(string className)
        {
            // Get teacher's assigned classes
            var assignedClasses = await GetAssignedClassesAsync();
            
            // Filter by the specified class and return sections
            var sections = assignedClasses
                .Where(c => c.Class == className && c.Section != null)
                .Select(c => c.Section!)
                .Distinct()
                .OrderBy(s => s)
                .ToList();

            // If teacher has "all sections" (null) for this class, get all sections from students
            if (assignedClasses.Any(c => c.Class == className && c.Section == null))
            {
                var allStudents = await _studentRepository.GetAllAsync();
                var studentSections = allStudents
                    .Where(s => s.Class == className && !string.IsNullOrEmpty(s.Section))
                    .Select(s => s.Section)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                // Merge with assigned sections
                sections = sections.Union(studentSections).OrderBy(s => s).ToList();
            }

            return Ok(sections);
        }
    }
}