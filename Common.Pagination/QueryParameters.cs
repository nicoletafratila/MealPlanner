namespace Common.Pagination
{
    public class QueryParameters<TItem>
    {
        private const int MaxPageSize = int.MaxValue;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = MaxPageSize;
        public IEnumerable<FilterItem>? Filters { get; set; }
        public IEnumerable<SortingModel> Sorting { get; init; } = [];

        public QueryParameters()
        {
            if (PageNumber <= 0) PageNumber = 1;
            if (PageSize <= 0 || PageSize > MaxPageSize) PageSize = MaxPageSize;
        }
    }
}
