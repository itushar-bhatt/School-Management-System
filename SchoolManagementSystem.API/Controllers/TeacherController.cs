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
            
            // If specific class/section is requested, check if teacher is assigned
            if (!string.IsNullOrEmpty(className))
            {
                var isAssigned = assignedClasses.Any(c => 
                    c.Class == className && 
                    (c.Section == null || c.Section == section));

                if (!isAssigned)
                {
                    return Ok(new 
                    { 
                        message = $"You are not assigned to Class {className}" + 
                            (!string.IsNullOrEmpty(section) ? $" Section {section}" : "") +
                            ". Please contact the admin to update your class assignments.",
                        students = new List<object>()
                    });
                }

                // Teacher is assigned - get students from this class/section
                IEnumerable<Domain.Entities.Student> students;
                if (!string.IsNullOrEmpty(section))
                {
                    students = await _studentRepository.GetByClassAndSectionAsync(className, section);
                }
                else
                {
                    var allStudents = await _studentRepository.GetAllAsync();
                    students = allStudents.Where(s => s.Class == className);
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

            // No specific class requested - return all students from assigned classes
            if (!assignedClasses.Any())
            {
                return Ok(new List<object>());
            }

            var allStudentsList = await _studentRepository.GetAllAsync();
            var filteredStudents = allStudentsList.Where(s => 
                assignedClasses.Any(c => 
                    c.Class == s.Class && 
                    (c.Section == null || c.Section == s.Section)));

            var studentList = new List<object>();
            foreach (var student in filteredStudents)
            {
                var user = await _userManager.FindByIdAsync(student.UserId);
                studentList.Add(new
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

            return Ok(studentList);
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
                        fatherName = parent.FatherName,
                        motherName = parent.MotherName,
                        phone = parent.Phone,
                        address = parent.Address,
                        username = user?.UserName,
                        fullName = user?.FullName,
                        email = user?.Email
                    });
                }
            }

            return Ok(result);
        }

        [HttpGet("assigned-classes")]
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
    }
}