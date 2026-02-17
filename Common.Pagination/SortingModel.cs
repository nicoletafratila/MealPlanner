using BlazorBootstrap;

namespace Common.Pagination
{
    public class SortingModel
    {
        public string PropertyName { get; init; } = default!;
        public SortDirection Direction { get; init; }
    }
}
