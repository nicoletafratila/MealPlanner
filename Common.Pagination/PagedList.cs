namespace Common.Pagination
{
    public class PagedList<T>
    {
        public Metadata Metadata { get; set; } = default!;
        public List<T> Items { get; set; } = new();

        public PagedList()
        {
        }

        public PagedList(IEnumerable<T> items, Metadata metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            Items = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        }
    }
}
