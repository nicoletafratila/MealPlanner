namespace Common.Pagination
{
    public record PagedQueryResult<T>(IReadOnlyList<T> Items, int TotalCount, int Skip);
}
