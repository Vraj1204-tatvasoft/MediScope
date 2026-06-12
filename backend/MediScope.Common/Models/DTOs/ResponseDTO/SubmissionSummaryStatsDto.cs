namespace MediScope.Common.Models.DTOs.Response
{
    public class SubmissionSummaryStatsDto
    {
        public int TotalRecords { get; set; }
        public int Normal { get; set; }
        public int Elevated { get; set; }
        public int Critical { get; set; }
    }
}