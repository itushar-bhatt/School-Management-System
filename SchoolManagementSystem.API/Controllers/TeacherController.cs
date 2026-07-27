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
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Student role
                if (await _roleManager.RoleExistsAsync("Student"))
                {
                    await _userManager.AddToRoleAsync(user, "Student");
                }

                return Ok(new { message = $"Student '{model.Username}' created successfully" });
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
                student = new { student.Id, student.UserName, student.FullName },
                parent = new { parent.Id, parent.UserName, parent.FullName }
            });
        }
    }
}