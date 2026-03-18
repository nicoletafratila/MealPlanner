using Common.Models;

namespace Common.Pagination
{
    public static class Extensions
    {
        public static PagedList<T> ToPagedList<T>(
            this IEnumerable<T> source,
            int pageNumber,
            int pageSize)
            where T : BaseModel
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

            var list = source as IList<T> ?? source.ToList();
            var totalCount = list.Count;

            var metadata = Metadata.Create(pageNumber, pageSize, totalCount);

            int skip;
            try
            {
                skip = checked((pageNumber - 1) * pageSize);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("The combination of pageNumber and pageSize caused an overflow.", ex);
            }

            var items = skip >= totalCount ? [] : list.Skip(skip).Take(pageSize).ToList();
            var startIndex = skip;
            for (var i = 0; i < items.Count; i++)
            {
                items[i].Index = startIndex + i + 1;
            }

            return new PagedList<T>(items, metadata);
        }
    }
}
