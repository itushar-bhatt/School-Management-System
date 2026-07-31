namespace SchoolManagementSystem.Application.DTOs
{
    public class AssignClassRequest
    {
        public string Class { get; set; } = string.Empty;
        public string? Section { get; set; }
    }
}