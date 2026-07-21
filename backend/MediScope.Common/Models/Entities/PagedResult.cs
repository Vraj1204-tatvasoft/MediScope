
namespace MediScope.Common.Models.Pagination
{
    /// Generic paged response wrapper returned by all paginated endpoints.
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }   // total records in DB
        public int PageNumber { get; set; }   // current page (1-based)
        public int PageSize { get; set; }   // records per page
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
        public DTOs.Response.SubmissionSummaryStatsDto SummaryStats { get; set; } = new();
        public int NormalCount { get; set; }
        public int ElevatedCount { get; set; }
        public int CriticalCount { get; set; }
    }
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 7;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? 7 : value;
        }
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Source { get; set; }
        public string? SortBy { get; set; } = "date";
        public string? SortDir { get; set; } = "desc";
    }
}