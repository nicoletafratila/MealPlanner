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

            int skip;
            try
            {
                skip = checked((pageNumber - 1) * pageSize);
            }
            catch (OverflowException ex)
            {
                throw new OverflowException("The combination of pageNumber and pageSize caused an overflow.", ex);
            }

            // Fetch one row past the page to learn whether more data exists, so the common
            // "everything fits on one page" case can skip the separate COUNT query below.
            var probe = await source.Skip(skip).Take(pageSize + 1).ToListAsync(cancellationToken);

            if (probe.Count > 0 && probe.Count <= pageSize)
            {
                return new PagedQueryResult<T>(probe, skip + probe.Count, skip);
            }

            if (probe.Count == 0 && skip == 0)
            {
                return new PagedQueryResult<T>([], 0, skip);
            }

            // The page is full (more pages may follow) or empty with skip > 0 (the requested
            // page may be past the last one) - either way the exact total is unknowable from
            // the probe alone.
            var totalCount = await source.CountAsync(cancellationToken);
            var items = probe.Count > pageSize ? probe.Take(pageSize).ToList() : probe;

            return new PagedQueryResult<T>(items, totalCount, skip);
        }
    }
}
