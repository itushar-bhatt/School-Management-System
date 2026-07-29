namespace SchoolManagementSystem.Application.DTOs
{
    public class CreateStudentRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        
        // Student class and section
        public string? Class { get; set; }
        public string? Section { get; set; }
        
        // Optional parent creation
        public bool CreateParent { get; set; } = false;
        public string? ParentUsername { get; set; }
        public string? ParentEmail { get; set; }
        public string? ParentFullName { get; set; }
        public string? ParentPassword { get; set; }
    }
}
