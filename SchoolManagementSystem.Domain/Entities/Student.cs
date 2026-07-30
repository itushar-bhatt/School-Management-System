namespace SchoolManagementSystem.Domain.Entities
{
    public class Student
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual User? User { get; set; }
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
}
