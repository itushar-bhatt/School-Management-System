using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentDashboardService _studentDashboardService;

        public StudentController(IStudentDashboardService studentDashboardService)
        {
            _studentDashboardService = studentDashboardService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetStudentDashboard()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var dashboard = await _studentDashboardService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }
    }
}
