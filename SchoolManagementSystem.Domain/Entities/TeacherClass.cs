namespace SchoolManagementSystem.Domain.Entities
{
    public class TeacherClass
    {
        public string Id { get; set; } = string.Empty;
        public string TeacherId { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string? Section { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}