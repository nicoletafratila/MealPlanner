using BlazorBootstrap;
using System.Linq.Expressions;

namespace Common.Pagination
{
    public static class EnumerableExtensions
    {
        public static IQueryable<TItem> ApplySorting<TItem>(this IQueryable<TItem> source, IEnumerable<SortingItem<TItem>> sortingItems)
        {
            bool firstSort = true;
            foreach (var sort in sortingItems)
            {
                var param = Expression.Parameter(typeof(TItem));
                var property = Expression.Property(param, sort.SortString);
                var lambda = Expression.Lambda(property, param);

                string methodName;

                if (firstSort)
                {
                    methodName = sort.SortDirection == SortDirection.Descending ? "OrderByDescending" : "OrderBy";
                    firstSort = false;
                }
                else
                {
                    methodName = sort.SortDirection == SortDirection.Descending ? "ThenByDescending" : "ThenBy";
                }

                var method = typeof(Queryable).GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .Single()
                    .MakeGenericMethod(typeof(TItem), property.Type);

                source = (IQueryable<TItem>)method.Invoke(null, new object[] { source, lambda });
            }

            return source;
        }
    }
}
