namespace SchoolManagementSystem.Application.DTOs
{
    public class AdmissionRequest
    {
        public StudentAdmissionInfo Student { get; set; } = new StudentAdmissionInfo();
        public ParentAdmissionInfo Parent { get; set; } = new ParentAdmissionInfo();
    }

    public class StudentAdmissionInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }

    public class ParentAdmissionInfo
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Occupation { get; set; }
    }
}
