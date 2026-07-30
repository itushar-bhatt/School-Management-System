namespace SchoolManagementSystem.Application.DTOs
{
    public class ParentProfile
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        
        // User information
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}