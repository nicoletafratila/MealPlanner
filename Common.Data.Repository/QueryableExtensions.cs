using Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Common.Data.Repository
{
    public static class QueryableExtensions
    {
        public static async Task<PagedQueryResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(source);

            var totalCount = await source.CountAsync(cancellationToken);

            int skip;
            try
            {
                skip = checked((pageNumber - 1) * pageSize);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("The combination of pageNumber and pageSize caused an overflow.", ex);
            }

            var items = skip >= totalCount
                ? []
                : await source.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

            return new PagedQueryResult<T>(items, totalCount, skip);
        }
    }
}
