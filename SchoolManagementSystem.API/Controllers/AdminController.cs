using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            if (!User.Identity.IsAuthenticated || !User.IsInRole("Admin"))
            {
                return Unauthorized(new { message = "Access denied" });
            }

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
                    "View all roles"
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
        public async Task<IActionResult> AddUser([FromBody] AddUserViewModel model)
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
                if (!string.IsNullOrEmpty(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                return Ok(new { message = $"User '{model.Username}' created successfully with role '{model.Role}'" });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }


        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
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

    public class AddUserViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string Role { get; set; } = "Student";
    }
}
