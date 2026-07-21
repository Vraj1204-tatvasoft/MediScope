using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Response
{
    public class DbPagedAdmission
    {
        public Guid Id { get; set; }
        public string Admission_Number { get; set; }
        public string Patient_Name { get; set; }
        public string Doctor_Name { get; set; }
        public string Ward_Name { get; set; }
        public string Room_Number { get; set; }
        public string Bed_Number { get; set; }
        public DateTime Admission_Date { get; set; }
        public int Status { get; set; }
        public long Total_Count { get; set; }
    }
    public class AdmissionSummaryDto
    {
        public Guid Id { get; set; }
        public string AdmissionNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string WardName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string BedNumber { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public int Status { get; set; }
    }
}