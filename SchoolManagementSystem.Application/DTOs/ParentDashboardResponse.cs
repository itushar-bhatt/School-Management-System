namespace SchoolManagementSystem.Application.DTOs
{
    public class ParentDashboardResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<StudentInfo> Children { get; set; } = new List<StudentInfo>();
    }

    public class StudentInfo
    {
        public string Id { get; set; } = string.Empty;
        public string AdmissionNo { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
    }
}
