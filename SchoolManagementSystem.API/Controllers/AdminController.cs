using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Interfaces;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITeacherClassRepository _teacherClassRepository;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITeacherClassRepository teacherClassRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _teacherClassRepository = teacherClassRepository;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            
            return Ok(new
            {
                message = "Hey Admin!",
                username = user?.UserName ?? User.Identity.Name,
                email = user?.Email,
                fullName = user?.FullName,
                role = "Admin",
                features = new[]
                {
                    "Add new users (Teachers, Students, Parents)",
                    "Manage all users",
                    "Delete users",
                    "View all roles",
                    "Assign classes to teachers",
                    "Manage teacher class assignments"
                }
            });
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var userViewModels = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new
                {
                    id = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName,
                    roles = roles.ToList()
                });
            }

            return Ok(userViewModels);
        }

        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] AddUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Restrict: Admin cannot create other Admin users
            if (model.Role == "Admin")
            {
                return BadRequest(new { message = "Cannot create Admin users. Only Teachers, Students, and Parents can be created." });
            }

            // Validate role
            var validRoles = new[] { "Student", "Teacher", "Parent" };
            if (!validRoles.Contains(model.Role))
            {
                return BadRequest(new { message = "Invalid role. Allowed roles: Student, Teacher, Parent" });
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
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.Role));
                }
                await _userManager.AddToRoleAsync(user, model.Role);

                return Ok(new { message = $"User '{model.Username}' created successfully with role '{model.Role}'" });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        // ============ TEACHER CLASS ASSIGNMENT ENDPOINTS ============

        // Create teacher WITH class assignments at registration time
        [HttpPost("users/teacher")]
        public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
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
                // Assign Teacher role
                if (!await _roleManager.RoleExistsAsync("Teacher"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Teacher"));
                }
                await _userManager.AddToRoleAsync(user, "Teacher");

                // Assign classes to teacher
                var assignedClasses = new List<object>();
                foreach (var assignment in model.ClassAssignments)
                {
                    // Check if assignment already exists
                    var exists = await _teacherClassRepository.ExistsAsync(user.Id, assignment.Class, assignment.Section);
                    if (!exists)
                    {
                        var teacherClass = new TeacherClass
                        {
                            Id = Guid.NewGuid().ToString(),
                            TeacherId = user.Id,
                            Class = assignment.Class,
                            Section = assignment.Section,
                            AssignedDate = DateTime.UtcNow,
                            IsActive = true
                        };

                        await _teacherClassRepository.AddAsync(teacherClass);

                        assignedClasses.Add(new
                        {
                            teacherClass.Id,
                            teacherClass.Class,
                            teacherClass.Section
                        });
                    }
                }

                return Ok(new
                {
                    message = $"Teacher '{model.Username}' created successfully with {assignedClasses.Count} class assignment(s)",
                    teacher = new
                    {
                        user.Id,
                        user.UserName,
                        user.FullName,
                        user.Email
                    },
                    classAssignments = assignedClasses
                });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        // Assign a new class to an existing teacher
        [HttpPost("teachers/{teacherId}/classes")]
        public async Task<IActionResult> AssignClassToTeacher(string teacherId, [FromBody] AssignClassRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Verify teacher exists
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Verify user is a teacher
            var roles = await _userManager.GetRolesAsync(teacher);
            if (!roles.Contains("Teacher"))
            {
                return BadRequest(new { message = "User is not a Teacher" });
            }

            // Check if assignment already exists
            var exists = await _teacherClassRepository.ExistsAsync(teacherId, model.Class, model.Section);
            if (exists)
            {
                return BadRequest(new { message = "This class assignment already exists for this teacher" });
            }

            // Create new assignment
            var teacherClass = new TeacherClass
            {
                Id = Guid.NewGuid().ToString(),
                TeacherId = teacherId,
                Class = model.Class,
                Section = model.Section,
                AssignedDate = DateTime.UtcNow,
                IsActive = true
            };

            await _teacherClassRepository.AddAsync(teacherClass);

            return Ok(new
            {
                message = "Class assigned to teacher successfully",
                assignment = new
                {
                    teacherClass.Id,
                    teacherClass.TeacherId,
                    teacherClass.Class,
                    teacherClass.Section,
                    teacherClass.AssignedDate
                }
            });
        }

        // Update/reassign a class assignment
        [HttpPut("teachers/{teacherId}/classes/{assignmentId}")]
        public async Task<IActionResult> UpdateTeacherClassAssignment(string teacherId, string assignmentId, [FromBody] AssignClassRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Get the assignment
            var assignment = await _teacherClassRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound(new { message = "Class assignment not found" });
            }

            // Verify the assignment belongs to the specified teacher
            if (assignment.TeacherId != teacherId)
            {
                return BadRequest(new { message = "This assignment does not belong to the specified teacher" });
            }

            // Check if the new class/section combination already exists for this teacher (excluding current)
            var exists = await _teacherClassRepository.ExistsAsync(teacherId, model.Class, model.Section);
            if (exists && assignment.Class != model.Class || assignment.Section != model.Section)
            {
                return BadRequest(new { message = "This class assignment already exists for this teacher" });
            }

            // Update the assignment
            assignment.Class = model.Class;
            assignment.Section = model.Section;

            await _teacherClassRepository.UpdateAsync(assignment);

            return Ok(new
            {
                message = "Class assignment updated successfully",
                assignment = new
                {
                    assignment.Id,
                    assignment.TeacherId,
                    assignment.Class,
                    assignment.Section,
                    assignment.AssignedDate
                }
            });
        }

        // Delete a class assignment
        [HttpDelete("teachers/{teacherId}/classes/{assignmentId}")]
        public async Task<IActionResult> DeleteTeacherClassAssignment(string teacherId, string assignmentId)
        {
            // Get the assignment
            var assignment = await _teacherClassRepository.GetByIdAsync(assignmentId);
            if (assignment == null)
            {
                return NotFound(new { message = "Class assignment not found" });
            }

            // Verify the assignment belongs to the specified teacher
            if (assignment.TeacherId != teacherId)
            {
                return BadRequest(new { message = "This assignment does not belong to the specified teacher" });
            }

            await _teacherClassRepository.DeleteAsync(assignmentId);

            return Ok(new { message = "Class assignment deleted successfully" });
        }

        // Get all classes assigned to a teacher
        [HttpGet("teachers/{teacherId}/classes")]
        public async Task<IActionResult> GetTeacherClasses(string teacherId)
        {
            // Verify teacher exists
            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var assignments = await _teacherClassRepository.GetByTeacherIdAsync(teacherId);

            var result = assignments.Select(a => new
            {
                a.Id,
                a.TeacherId,
                a.Class,
                a.Section,
                a.AssignedDate,
                a.IsActive
            });

            return Ok(result);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // If user is a teacher, delete their class assignments
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Teacher"))
            {
                await _teacherClassRepository.DeleteByTeacherIdAsync(id);
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(new { message = "User deleted successfully" });
            }

            return BadRequest(new { message = "Failed to delete user" });
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }
    }
}