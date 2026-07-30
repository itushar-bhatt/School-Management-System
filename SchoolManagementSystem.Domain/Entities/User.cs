namespace SchoolManagementSystem.Domain.Entities
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        
        // Student-specific properties
        public string? Class { get; set; }
        public string? Section { get; set; }
    }
}