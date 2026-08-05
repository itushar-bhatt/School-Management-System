namespace SchoolManagementSystem.Domain.Entities
{
    public class AdmissionSequence
    {
        public string Id { get; set; } = string.Empty;
        public string AcademicSession { get; set; } = string.Empty;
        public int LastNumber { get; set; }
    }
}