namespace Common.Pagination
{
    public class PagedList<T>(List<T> items, Metadata metadata)
    {
        public Metadata? Metadata { get; set; } = metadata;
        public List<T>? Items { get; set; } = items;
    }
}
