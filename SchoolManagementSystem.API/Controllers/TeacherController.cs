using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
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
                    "Create assignments and tests"
                }
            });
        }
    }
}