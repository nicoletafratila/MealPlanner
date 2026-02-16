using System.Linq.Expressions;
using System.Reflection;
using BlazorBootstrap;

namespace Common.Pagination
{
    public sealed class QueryParameters<TItem> : GridSettings
    {
        private const int MaxPageSize = int.MaxValue;

        public IEnumerable<SortingModel>? Sorting { get; init; }

        public QueryParameters()
        {
            if (PageNumber <= 0)
                PageNumber = 1;

            if (PageSize <= 0)
            {
                PageSize = MaxPageSize;
            }
            else if (PageSize > MaxPageSize)
            {
                PageSize = MaxPageSize;
            }
        }

        public static SortingModel ToModel(SortingItem<TItem> sortingItem)
        {
            ArgumentNullException.ThrowIfNull(sortingItem);

            MemberExpression? memberExpr = sortingItem.SortKeySelector.Body as MemberExpression;

            if (memberExpr == null && sortingItem.SortKeySelector.Body is UnaryExpression unaryExpr)
            {
                memberExpr = unaryExpr.Operand as MemberExpression;
            }

            var propertyName = memberExpr?.Member.Name ?? throw new InvalidOperationException("SortKeySelector must point to a property.");

            return new SortingModel
            {
                PropertyName = propertyName,
                Direction = sortingItem.SortDirection
            };
        }

        public static SortingItem<TItem> FromModel(SortingModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            var parameter = Expression.Parameter(typeof(TItem), "x");

            var property = typeof(TItem).GetProperty(model.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

            if (property == null)
                throw new InvalidOperationException($"Property '{model.PropertyName}' not found in type {typeof(TItem).Name}.");

            var memberAccess = Expression.Property(parameter, property);

            Expression body = property.PropertyType.IsValueType ? Expression.Convert(memberAccess, typeof(IComparable)) : memberAccess;

            var lambda = Expression.Lambda<Func<TItem, IComparable>>(body, parameter);

            return new SortingItem<TItem>(property.Name, lambda, model.Direction);
        }
    }
}
