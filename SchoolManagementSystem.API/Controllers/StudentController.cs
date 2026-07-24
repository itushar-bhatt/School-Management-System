using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetStudentDashboard()
        {
            return Ok(new
            {
                message = "Hello Student!",
                username = User.Identity.Name,
                role = "Student",
                features = new[]
                {
                    "View class schedule",
                    "Check assignments and homework",
                    "View grades and report cards",
                    "Check attendance records",
                    "Access study materials"
                }
            });
        }
    }
}