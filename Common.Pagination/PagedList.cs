namespace Common.Pagination
{
    public class PagedList<T>
    {
        public Metadata? Metadata { get; set; }
        public List<T>? Items { get; set; }

        public PagedList(List<T> items, Metadata metadata)
        {
            Metadata = metadata;
            Items = items;
        }
    }
}
