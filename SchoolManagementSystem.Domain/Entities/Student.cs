namespace SchoolManagementSystem.Domain.Entities
{
    public class Student
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        
        // Student Details
        public string Name { get; set; } = string.Empty;
        public DateOnly DOB { get; set; } = DateOnly.MinValue;
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        
        // Academic Details
        public string AdmissionNo { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
        public string AcademicSession { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
}