namespace MediScope.Common.Models.Entities
{
    public class Doctor : BaseEntity
    {
        public Guid UserId { get; set; }
        public string? Specialization { get; set; }
        public string LicenseNumber { get; set; } = null!;
        public string? ContactNumber { get; set; }
        public string? Bio { get; set; }
        public string? Hospital { get; set; }
        public int? YearsExperience { get; set; }
        public User User { get; set; } = null!;
        public ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
        public ICollection<MedicalDocument> MedicalDocuments { get; set; } = new List<MedicalDocument>();
    }
}