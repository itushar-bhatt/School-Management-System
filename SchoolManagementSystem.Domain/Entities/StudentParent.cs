namespace SchoolManagementSystem.Domain.Entities
{
    public class StudentParent
    {
        public string Id { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string ParentId { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Parent? Parent { get; set; }
    }
}
