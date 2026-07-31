namespace SchoolManagementSystem.Application.DTOs
{
    public class CreateTeacherRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<AssignClassRequest> ClassAssignments { get; set; } = new List<AssignClassRequest>();
    }
}