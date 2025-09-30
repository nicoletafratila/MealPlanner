using BlazorBootstrap;

namespace Common.Pagination
{
    public class SortingModel
    {
        public required string PropertyName { get; set; }
        public SortDirection Direction { get; set; }
    }
}
