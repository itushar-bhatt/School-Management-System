namespace SchoolManagementSystem.Domain.Entities
{
    public class Parent
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
}
