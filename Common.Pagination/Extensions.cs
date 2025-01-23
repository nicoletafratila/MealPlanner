using BlazorBootstrap;
using Common.Models;
using System.Linq.Expressions;

namespace Common.Pagination
{
    public static class Extensions
    {
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize) where T : BaseModel
        {
            var metadata = new Metadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(source.Count() / (double)pageSize),
                TotalCount = source.Count()
            };
            var items = source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize).ToList();

            foreach (var item in items)
            {
                item.Index = pageSize * (pageNumber - 1) + items.IndexOf(item)+1;
            }
            return new PagedList<T>(items, metadata);
        }
    }

    public static class FilterExtensions
    {
        public static Func<T, bool> ConvertFilterItemToFunc<T>(this FilterItem filter)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, filter.PropertyName);
            var constant = Expression.Constant(filter.Value);

            Expression body = filter.Operator switch
            {
                FilterOperator.Equals => Expression.Equal(member, constant),
                FilterOperator.NotEquals => Expression.NotEqual(member, constant),
                FilterOperator.Contains => Expression.Call(
                    member,
                    typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) })!,
                    constant,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)
                ),
                FilterOperator.StartsWith => Expression.Call(
                    member,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) })!,
                    constant,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)
                ),
                FilterOperator.EndsWith => Expression.Call(
                    member,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) })!,
                    constant,
                    Expression.Constant(StringComparison.OrdinalIgnoreCase)
                ),
                _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported")
            };

            return Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
        }
    }

    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string propertyName, SortDirection sortDirection)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Property name cannot be null or empty.", nameof(propertyName));
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var lambda = Expression.Lambda(property, parameter);

            string methodName = sortDirection == SortDirection.Ascending ? "OrderBy" : "OrderByDescending";
            var resultExpression = Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.Type }, source.Expression, Expression.Quote(lambda));

            return source.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
