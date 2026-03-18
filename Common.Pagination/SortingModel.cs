using BlazorBootstrap;

namespace Common.Pagination
{
    public class SortingModel
    {
        public required string PropertyName { get; init; }

        public SortDirection Direction { get; init; } = SortDirection.Ascending;

        public override string ToString() => $"{PropertyName} ({Direction})";
    }
}