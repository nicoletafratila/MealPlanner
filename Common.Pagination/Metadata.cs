namespace Common.Pagination
{
    public class Metadata
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }
}