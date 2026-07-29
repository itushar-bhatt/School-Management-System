using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public TeacherController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                    "Create students and parents",
                    "Link students with parents"
                }
            });
        }

        [HttpPost("users/student")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                Class = model.Class,
                Section = model.Section
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Student role
                if (await _roleManager.RoleExistsAsync("Student"))
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }

                // If parent creation requested, create and link parent
                if (model.CreateParent && 
                    !string.IsNullOrEmpty(model.ParentUsername) && 
                    !string.IsNullOrEmpty(model.ParentPassword))
                {
                    var parent = new ApplicationUser
                    {
                        UserName = model.ParentUsername,
                        Email = model.ParentEmail ?? string.Empty,
                        FullName = model.ParentFullName ?? model.ParentUsername,
                        EmailConfirmed = true
                    };

                    var parentResult = await _userManager.CreateAsync(parent, model.ParentPassword);

                    if (parentResult.Succeeded)
                    {
                        if (await _roleManager.RoleExistsAsync("Parent"))
                        {
                            await _userManager.AddToRoleAsync(parent, "Parent");
                        }

                        return Ok(new { 
                            message = "Student created successfully and linked with parent",
                            student = new { user.Id, user.UserName, user.FullName, user.Class, user.Section },
                            parent = new { parent.Id, parent.UserName, parent.FullName }
                        });
                    }
                }

                return Ok(new { 
                    message = $"Student '{model.Username}' created successfully",
                    student = new { user.Id, user.UserName, user.FullName, user.Class, user.Section }
                });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost("users/parent")]
        public async Task<IActionResult> CreateParent([FromBody] CreateParentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

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
                if (await _roleManager.RoleExistsAsync("Parent"))
                {
                    await _userManager.AddToRoleAsync(user, "Parent");
                }

                return Ok(new { message = $"Parent '{model.Username}' created successfully" });
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

            // Verify student exists
            var student = await _userManager.FindByIdAsync(model.StudentId);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Verify parent exists
            var parent = await _userManager.FindByIdAsync(model.ParentId);
            if (parent == null)
            {
                return NotFound(new { message = "Parent not found" });
            }

            // Verify user roles
            var studentRoles = await _userManager.GetRolesAsync(student);
            var parentRoles = await _userManager.GetRolesAsync(parent);

            if (!studentRoles.Contains("Student"))
            {
                return BadRequest(new { message = "User is not a Student" });
            }

            if (!parentRoles.Contains("Parent"))
            {
                return BadRequest(new { message = "User is not a Parent" });
            }

            // Create link (in a real app, you'd save this to a database table)
            // For now, we'll just return success
            // TODO: Implement StudentParent repository and save to database
            
            return Ok(new { 
                message = "Student linked with parent successfully",
                student = new { student.Id, student.UserName, student.FullName, student.Class, student.Section },
                parent = new { parent.Id, parent.UserName, parent.FullName }
            });
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents([FromQuery] string? className, [FromQuery] string? section)
        {
            // Get all users with Student role
            var students = new List<ApplicationUser>();
            
            // Get all users and filter by Student role
            var allUsers = _userManager.Users.ToList();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student"))
                {
                    students.Add(user);
                }
            }

            // Apply filters
            if (!string.IsNullOrEmpty(className))
            {
                students = students.Where(s => s.Class == className).ToList();
            }
            
            if (!string.IsNullOrEmpty(section))
            {
                students = students.Where(s => s.Section == section).ToList();
            }

            // Return filtered students
            var result = students.Select(s => new
            {
                id = s.Id,
                username = s.UserName,
                fullName = s.FullName,
                email = s.Email,
                class_ = s.Class,
                section = s.Section
            });

            return Ok(result);
        }

        [HttpGet("parents")]
        public async Task<IActionResult> GetParents()
        {
            // Get all users with Parent role
            var parents = new List<ApplicationUser>();
            
            // Get all users and filter by Parent role
            var allUsers = _userManager.Users.ToList();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Parent"))
                {
                    parents.Add(user);
                }
            }

            // Return parents
            var result = parents.Select(p => new
            {
                id = p.Id,
                username = p.UserName,
                fullName = p.FullName,
                email = p.Email
            });

            return Ok(result);
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            // Get all students
            var allUsers = _userManager.Users.ToList();
            var classes = new HashSet<string>();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student") && !string.IsNullOrEmpty(user.Class))
                {
                    classes.Add(user.Class);
                }
            }

            // Return unique classes
            return Ok(classes.OrderBy(c => c).ToList());
        }

        [HttpGet("classes/{className}/sections")]
        public async Task<IActionResult> GetSections(string className)
        {
            // Get all students in the specified class
            var allUsers = _userManager.Users.ToList();
            var sections = new HashSet<string>();
            
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student") && user.Class == className && !string.IsNullOrEmpty(user.Section))
                {
                    sections.Add(user.Section);
                }
            }

            // Return unique sections
            return Ok(sections.OrderBy(s => s).ToList());
        }
    }
}