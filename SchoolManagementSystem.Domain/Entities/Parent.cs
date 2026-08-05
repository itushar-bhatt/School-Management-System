namespace SchoolManagementSystem.Domain.Entities
{
    public class Parent
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        
        // Parent Details
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
}