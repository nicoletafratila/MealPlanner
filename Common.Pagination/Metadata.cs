namespace Common.Pagination
{
    public class Metadata
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public bool ShowGoToFirst => PageNumber != 1;
        public bool ShowGoToLast => PageNumber != TotalPages;
    }
}