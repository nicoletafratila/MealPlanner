using System.Linq.Expressions;
using BlazorBootstrap;

namespace Common.Pagination
{
    public class QueryParameters<TItem> : GridSettings
    {
        private const int MaxPageSize = int.MaxValue;
        public IEnumerable<SortingModel>? Sorting { get; init; }

        public QueryParameters()
        {
            PageNumber = 1;
            PageSize = PageSize > MaxPageSize ? MaxPageSize : PageSize;
        }

        public static SortingModel ToModel(SortingItem<TItem> sortingItem)
        {
            var memberExpr = sortingItem.SortKeySelector.Body as MemberExpression;
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
            var parameter = Expression.Parameter(typeof(TItem), "x");
            var property = typeof(TItem).GetProperty(model.PropertyName);
            if (property == null)
                throw new InvalidOperationException($"Property '{model.PropertyName}' not found in type {typeof(TItem).Name}.");

            var memberAccess = Expression.MakeMemberAccess(parameter, property);

            Expression body = property.PropertyType.IsValueType
                            ? Expression.Convert(memberAccess, typeof(IComparable))
                            : (Expression)memberAccess;

            var lambda = Expression.Lambda<Func<TItem, IComparable>>(body, parameter);

            return new SortingItem<TItem>(model.PropertyName, lambda, model.Direction);
        }
    }
}
