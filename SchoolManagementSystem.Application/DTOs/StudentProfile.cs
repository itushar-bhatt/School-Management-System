namespace SchoolManagementSystem.Application.DTOs
{
    public class StudentProfile
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public bool IsActive { get; set; }
        
        // User information
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}