using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Application.DTOs;
using SchoolManagementSystem.Application.Interfaces;

namespace SchoolManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Parent")]
    public class ParentController : ControllerBase
    {
        private readonly IParentDashboardService _parentDashboardService;

        public ParentController(IParentDashboardService parentDashboardService)
        {
            _parentDashboardService = parentDashboardService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetParentDashboard()
        {
            var userId = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var dashboard = await _parentDashboardService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }
    }
}