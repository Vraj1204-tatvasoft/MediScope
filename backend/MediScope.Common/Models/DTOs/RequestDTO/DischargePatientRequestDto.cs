using MediScope.Common.Models.Enums;
namespace MediScope.Common.Models.DTOs.Request
{
    public class DischargePatientRequestDto
    {
        public string DischargeNotes { get; set; } = string.Empty;
        public DateTime DischargeDate { get; set; }
    }
}