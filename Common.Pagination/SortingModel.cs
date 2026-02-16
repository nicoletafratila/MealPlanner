using BlazorBootstrap;

namespace Common.Pagination
{
    public sealed class SortingModel
    {
        public string PropertyName { get; init; } = default!;
        public SortDirection Direction { get; init; }
    }
}
