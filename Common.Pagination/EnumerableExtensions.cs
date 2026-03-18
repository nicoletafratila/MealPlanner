using System.Linq.Expressions;
using System.Reflection;
using BlazorBootstrap;

namespace Common.Pagination
{
    public static class EnumerableExtensions
    {
        public static IQueryable<TItem> ApplySorting<TItem>(
            this IQueryable<TItem> source,
            IEnumerable<SortingItem<TItem>>? sortingItems)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (sortingItems == null)
            {
                return source;
            }

            var sorts = sortingItems.ToList();
            if (sorts.Count == 0)
            {
                return source;
            }

            var parameter = Expression.Parameter(typeof(TItem), "x");
            var isFirst = true;
            var current = source.Expression;

            foreach (var sort in sorts)
            {
                if (string.IsNullOrWhiteSpace(sort.SortString))
                    throw new ArgumentException("SortString cannot be null or empty.", nameof(sortingItems));

                var propertyInfo = typeof(TItem).GetProperty(
                    sort.SortString,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) ?? throw new ArgumentException($"Property '{sort.SortString}' does not exist on type {typeof(TItem).Name}.", nameof(sortingItems));
                var property = Expression.Property(parameter, propertyInfo);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = isFirst
                    ? (sort.SortDirection == SortDirection.Descending ? "OrderByDescending" : "OrderBy")
                    : (sort.SortDirection == SortDirection.Descending ? "ThenByDescending" : "ThenBy");

                isFirst = false;

                current = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    [typeof(TItem), property.Type],
                    current,
                    Expression.Quote(lambda));
            }

            return source.Provider.CreateQuery<TItem>(current);
        }
    }
}