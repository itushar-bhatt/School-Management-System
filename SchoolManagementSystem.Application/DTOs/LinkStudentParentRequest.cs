namespace SchoolManagementSystem.Application.DTOs
{
    public class LinkStudentParentRequest
    {
        public string StudentId { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
    }
}