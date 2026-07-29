using Microsoft.AspNetCore.Identity;

namespace SchoolManagementSystem.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        
        // Student-specific properties
        public string? Class { get; set; }
        public string? Section { get; set; }
    }
}
