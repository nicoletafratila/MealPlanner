using System.Linq.Expressions;

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

                var member = PropertyPathExpression.Resolve(parameter, sort.PropertyName);
                var lambda = Expression.Lambda(member, parameter);

                var methodName = isFirst
                    ? (sort.Direction == SortDirection.Descending ? "OrderByDescending" : "OrderBy")
                    : (sort.Direction == SortDirection.Descending ? "ThenByDescending" : "ThenBy");

                isFirst = false;

                current = Expression.Call(
                    typeof(Queryable),
                    methodName,
                    [typeof(TItem), member.Type],
                    current,
                    Expression.Quote(lambda));
            }

            return source.Provider.CreateQuery<TItem>(current);
        }
    }
}