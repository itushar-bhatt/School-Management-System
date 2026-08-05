namespace SchoolManagementSystem.Application.DTOs
{
    public class AdmissionRequest
    {
        public StudentAdmissionInfo Student { get; set; } = new StudentAdmissionInfo();
        public ParentAdmissionInfo Parent { get; set; } = new ParentAdmissionInfo();
    }

    public class StudentAdmissionInfo
    {
        // Student Details
        public string Name { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        
        // Academic Details
        public string AcademicSession { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        
        // Login credentials
        public string Password { get; set; } = string.Empty;
    }

    public class ParentAdmissionInfo
    {
        // Parent Details
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        
        // Login credentials
        public string Password { get; set; } = string.Empty;
    }
}