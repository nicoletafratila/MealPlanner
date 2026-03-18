namespace Common.Pagination
{
    public class PagedList<T>
    {
        public Metadata Metadata { get; set; } = new();

        public List<T> Items { get; set; } = [];

        public int Count => Items.Count;

        public bool HasItems => Items.Count > 0;

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
