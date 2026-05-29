using System.Linq.Expressions;
using System.Reflection;

namespace Common.Pagination
{
    public static class EnumerableExtensions
    {
        public static IQueryable<TItem> ApplySorting<TItem>(
            this IQueryable<TItem> source,
            IEnumerable<SortingModel>? sortingModels)
        {
            ArgumentNullException.ThrowIfNull(source);

            if (sortingModels == null) return source;

            var sorts = sortingModels.ToList();
            if (sorts.Count == 0) return source;

            var parameter = Expression.Parameter(typeof(TItem), "x");
            var isFirst = true;
            var current = source.Expression;

            foreach (var sort in sorts)
            {
                if (string.IsNullOrWhiteSpace(sort.PropertyName))
                    throw new ArgumentException("SortString cannot be null or empty.", nameof(sortingModels));

                var propertyInfo = typeof(TItem).GetProperty(
                    sort.PropertyName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                    ?? throw new ArgumentException(
                        $"Property '{sort.PropertyName}' does not exist on type {typeof(TItem).Name}.",
                        nameof(sortingModels));

                var property = Expression.Property(parameter, propertyInfo);
                var lambda = Expression.Lambda(property, parameter);

                var methodName = isFirst
                    ? (sort.Direction == SortDirection.Descending ? "OrderByDescending" : "OrderBy")
                    : (sort.Direction == SortDirection.Descending ? "ThenByDescending" : "ThenBy");

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