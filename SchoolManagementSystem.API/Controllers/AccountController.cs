using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid request" });
            }

            // Auto-detect user: try Email first, then UserName (Phone/AdmissionNo)
            var user = await _userManager.FindByEmailAsync(model.Identifier);
            
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.Identifier);
            }

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Verify password using ASP.NET Identity
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Get the user's actual role
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName,
                    role = roles.FirstOrDefault(),
                    roles = roles.ToList()
                });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
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
                if (!string.IsNullOrEmpty(model.Role) && await _roleManager.RoleExistsAsync(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok(new { message = "Registration successful" });
            }

            return BadRequest(new { message = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logout successful" });
        }
    }
}