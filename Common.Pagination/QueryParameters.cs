using BlazorBootstrap;

namespace Common.Pagination
{
    public class QueryParameters
    {
        private int _pageSize = 10;
        private const int MaxPageSize = int.MaxValue;

        public IEnumerable<FilterItem>? Filters;
        public string? SortString { get; set; }
        public SortDirection SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}
