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
                    "Create teachers with class assignments",
                    "Manage teacher class assignments",
                    "View all users",
                    "Delete users"
                }
            });
        }

        // ============ USER MANAGEMENT ============

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

        // Delete any user
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // If user is a teacher, delete their class assignments too
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

        // ============ TEACHER CLASS ASSIGNMENT MANAGEMENT ============

        // Get all classes assigned to a teacher
        [HttpGet("teachers/{teacherId}/classes")]
        public async Task<IActionResult> GetTeacherClasses(string teacherId)
        {
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

        // Replace ALL class assignments for a teacher (bulk update)
        [HttpPut("teachers/{teacherId}/classes")]
        public async Task<IActionResult> ReplaceTeacherClasses(string teacherId, [FromBody] List<AssignClassRequest> assignments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            var teacher = await _userManager.FindByIdAsync(teacherId);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            var roles = await _userManager.GetRolesAsync(teacher);
            if (!roles.Contains("Teacher"))
            {
                return BadRequest(new { message = "User is not a Teacher" });
            }

            // Create new assignment objects
            var newAssignments = assignments.Select(a => new TeacherClass
            {
                Id = Guid.NewGuid().ToString(),
                TeacherId = teacherId,
                Class = a.Class,
                Section = a.Section,
                AssignedDate = DateTime.UtcNow,
                IsActive = true
            }).ToList();

            // Replace all assignments (deletes old, adds new)
            await _teacherClassRepository.ReplaceByTeacherIdAsync(teacherId, newAssignments);

            return Ok(new
            {
                message = $"Teacher's class assignments updated successfully ({newAssignments.Count} assignments)",
                assignments = newAssignments.Select(a => new
                {
                    a.Id,
                    a.Class,
                    a.Section
                })
            });
        }

        // ============ ROLES ============

        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return Ok(roles);
        }
    }
}