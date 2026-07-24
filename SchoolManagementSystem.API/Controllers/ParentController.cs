using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class ParentController : ControllerBase
    {
        [HttpGet("dashboard")]
        public IActionResult GetParentDashboard()
        {
            return Ok(new
            {
                message = "Hello Parent!",
                username = User.Identity.Name,
                role = "Parent",
                features = new[]
                {
                    "View children's information",
                    "Check attendance records",
                    "View grades and report cards",
                    "Communicate with teachers",
                    "View school announcements"
                }
            });
        }
    }
}