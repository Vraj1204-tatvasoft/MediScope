namespace MediScope.Common.Models.Entities
{
    public class PatientCardToken : BaseEntity
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string RazorpayTokenId { get; set; } = string.Empty;
        public string Last4Digits { get; set; } = string.Empty;
        public string CardNetwork { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Patient Patient { get; set; } = null!;
    }
}