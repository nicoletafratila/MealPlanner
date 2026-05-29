using System.Linq.Expressions;
using System.Reflection;

namespace Common.Pagination
{
    public static class FilterExtensions
    {
        public static Func<T, bool> ConvertFilterItemToFunc<T>(this FilterItem filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            if (string.IsNullOrWhiteSpace(filter.PropertyName))
                throw new ArgumentException("PropertyName cannot be null or empty.", nameof(filter));

            var propertyInfo = typeof(T).GetProperty(
                                   filter.PropertyName,
                                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                               ?? throw new ArgumentException(
                                   $"Property {filter.PropertyName} does not exist on type {typeof(T).Name}",
                                   nameof(filter));

            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, propertyInfo);
            var propertyType = propertyInfo.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            bool isNullable = underlyingType != propertyType;

            object? filterValue = filter.Value;
            if (filterValue is not null && underlyingType != filterValue.GetType())
            {
                if (underlyingType.IsEnum && filterValue is string enumString)
                    filterValue = Enum.Parse(underlyingType, enumString, ignoreCase: true);
                else
                    filterValue = Convert.ChangeType(filterValue, underlyingType);
            }

            var constant = isNullable && filterValue is not null
                ? Expression.Constant(filterValue, underlyingType)
                : Expression.Constant(filterValue, propertyType);

            var valueAccess = isNullable
                ? Expression.Property(member, nameof(Nullable<int>.Value))
                : (Expression)member;

            var hasValue = isNullable
                ? (Expression)Expression.Property(member, nameof(Nullable<int>.HasValue))
                : Expression.Constant(true);

            Expression body = filter.Operator switch
            {
                FilterOperator.Equals =>
                    propertyType == typeof(string)
                        ? BuildStringEqualsCall(member, Expression.Constant(filterValue, propertyType), filter.StringComparison)
                        : isNullable
                            ? Expression.AndAlso(hasValue, Expression.Equal(valueAccess, constant))
                            : Expression.Equal(member, constant),

                FilterOperator.NotEquals =>
                    propertyType == typeof(string)
                        ? Expression.Not(BuildStringEqualsCall(member, Expression.Constant(filterValue, propertyType), filter.StringComparison))
                        : isNullable
                            ? Expression.OrElse(Expression.Not(hasValue), Expression.NotEqual(valueAccess, constant))
                            : Expression.NotEqual(member, constant),

                FilterOperator.LessThan =>
                    isNullable
                        ? Expression.AndAlso(hasValue, Expression.LessThan(valueAccess, constant))
                        : Expression.LessThan(member, constant),

                FilterOperator.LessThanOrEquals =>
                    isNullable
                        ? Expression.AndAlso(hasValue, Expression.LessThanOrEqual(valueAccess, constant))
                        : Expression.LessThanOrEqual(member, constant),

                FilterOperator.GreaterThan =>
                    isNullable
                        ? Expression.AndAlso(hasValue, Expression.GreaterThan(valueAccess, constant))
                        : Expression.GreaterThan(member, constant),

                FilterOperator.GreaterThanOrEquals =>
                    isNullable
                        ? Expression.AndAlso(hasValue, Expression.GreaterThanOrEqual(valueAccess, constant))
                        : Expression.GreaterThanOrEqual(member, constant),

                FilterOperator.Contains =>
                    BuildStringMethodCall(member, Expression.Constant(filterValue, propertyType), nameof(string.Contains), filter.StringComparison),

                FilterOperator.StartsWith =>
                    BuildStringMethodCall(member, Expression.Constant(filterValue, propertyType), nameof(string.StartsWith), filter.StringComparison),

                FilterOperator.EndsWith =>
                    BuildStringMethodCall(member, Expression.Constant(filterValue, propertyType), nameof(string.EndsWith), filter.StringComparison),

                _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported")
            };

            return Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
        }

        private static BinaryExpression BuildStringEqualsCall(
            MemberExpression member, ConstantExpression constant, StringComparison comparison)
        {
            if (member.Type != typeof(string))
                throw new NotSupportedException("Operator Equals with StringComparison is only supported for string properties.");
            if (constant.Value is null)
                throw new ArgumentException("Filter value for operator Equals cannot be null.");

            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            var equalsMethod = typeof(string).GetMethod(nameof(string.Equals), [typeof(string), typeof(string), typeof(StringComparison)])
                ?? throw new InvalidOperationException("Could not find string.Equals(string, string, StringComparison) overload.");
            var call = Expression.Call(equalsMethod, member, constant, Expression.Constant(comparison));
            return Expression.AndAlso(notNull, call);
        }

        private static BinaryExpression BuildStringMethodCall(
            MemberExpression member, ConstantExpression constant, string methodName, StringComparison comparison)
        {
            if (member.Type != typeof(string))
                throw new NotSupportedException($"Operator {methodName} is only supported for string properties.");
            if (constant.Value is null)
                throw new ArgumentException($"Filter value for operator {methodName} cannot be null.");

            var method = typeof(string).GetMethod(methodName, [typeof(string), typeof(StringComparison)])
                ?? throw new InvalidOperationException($"Could not find string.{methodName}(string, StringComparison) overload.");
            var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
            var call = Expression.Call(member, method, constant, Expression.Constant(comparison));
            return Expression.AndAlso(notNull, call);
        }
    }
}
