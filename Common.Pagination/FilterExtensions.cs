using System.Linq.Expressions;
using System.Reflection;
using BlazorBootstrap;

namespace Common.Pagination
{
    public static class FilterExtensions
    {
        public static Func<T, bool> ConvertFilterItemToFunc<T>(this FilterItem filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            if (string.IsNullOrWhiteSpace(filter.PropertyName))
            {
                throw new ArgumentException("PropertyName cannot be null or empty.", nameof(filter));
            }

            var propertyInfo = typeof(T).GetProperty(
                                   filter.PropertyName,
                                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                               ?? throw new ArgumentException(
                                   $"Property {filter.PropertyName} does not exist on type {typeof(T).Name}",
                                   nameof(filter));

            var parameter = Expression.Parameter(typeof(T), "x");
            var member = Expression.Property(parameter, propertyInfo);
            var propertyType = propertyInfo.PropertyType;

            object? filterValue = filter.Value;
            if (filterValue is not null && propertyType != filterValue.GetType())
            {
                if (propertyType.IsEnum && filterValue is string enumString)
                {
                    filterValue = Enum.Parse(propertyType, enumString, ignoreCase: true);
                }
                else
                {
                    filterValue = Convert.ChangeType(filterValue, propertyType);
                }
            }

            var constant = Expression.Constant(filterValue, propertyType);

            Expression body = filter.Operator switch
            {
                FilterOperator.Equals =>
                    propertyType == typeof(string)
                        ? BuildStringEqualsCall(member, constant, filter.StringComparison)
                        : Expression.Equal(member, constant),

                FilterOperator.NotEquals =>
                    propertyType == typeof(string)
                        ? Expression.Not(BuildStringEqualsCall(member, constant, filter.StringComparison))
                        : Expression.NotEqual(member, constant),

                FilterOperator.Contains =>
                    BuildStringMethodCall(member, constant, nameof(string.Contains), filter.StringComparison),

                FilterOperator.StartsWith =>
                    BuildStringMethodCall(member, constant, nameof(string.StartsWith), filter.StringComparison),

                FilterOperator.EndsWith =>
                    BuildStringMethodCall(member, constant, nameof(string.EndsWith), filter.StringComparison),

                _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported")
            };

            return Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
        }

        private static BinaryExpression BuildStringEqualsCall(
            MemberExpression member,
            ConstantExpression constant,
            StringComparison comparison)
        {
            if (member.Type != typeof(string))
            {
                throw new NotSupportedException(
                    "Operator Equals with StringComparison is only supported for string properties.");
            }

            if (constant.Value is null)
            {
                throw new ArgumentException("Filter value for operator Equals cannot be null.");
            }

            // x.Property != null && string.Equals(x.Property, value, comparison)
            var notNull = Expression.NotEqual(
                member,
                Expression.Constant(null, typeof(string)));

            var equalsMethod = typeof(string).GetMethod(
                                   nameof(string.Equals),
                                   new[] { typeof(string), typeof(string), typeof(StringComparison) })
                               ?? throw new InvalidOperationException(
                                   "Could not find string.Equals(string, string, StringComparison) overload.");

            var call = Expression.Call(
                equalsMethod,
                member,
                constant,
                Expression.Constant(comparison));

            return Expression.AndAlso(notNull, call);
        }

        private static BinaryExpression BuildStringMethodCall(
            MemberExpression member,
            ConstantExpression constant,
            string methodName,
            StringComparison comparison)
        {
            if (member.Type != typeof(string))
            {
                throw new NotSupportedException(
                    $"Operator {methodName} is only supported for string properties.");
            }

            if (constant.Value is null)
            {
                throw new ArgumentException(
                    $"Filter value for operator {methodName} cannot be null.");
            }

            var method = typeof(string).GetMethod(
                             methodName,
                             new[] { typeof(string), typeof(StringComparison) })
                         ?? throw new InvalidOperationException(
                             $"Could not find string.{methodName}(string, StringComparison) overload.");

            // x.Property != null && x.Property.Method(value, comparison)
            var notNull = Expression.NotEqual(
                member,
                Expression.Constant(null, typeof(string)));

            var call = Expression.Call(
                member,
                method,
                constant,
                Expression.Constant(comparison));

            return Expression.AndAlso(notNull, call);
        }
    }

    //public static Func<T, bool> ConvertFilterItemToFunc<T>(this FilterItem filter)
    //{
    //    ArgumentNullException.ThrowIfNull(filter);

    //    if (string.IsNullOrWhiteSpace(filter.PropertyName))
    //        throw new ArgumentException("PropertyName cannot be null or empty.", nameof(filter));

    //    var propertyInfo = typeof(T).GetProperty(
    //                           filter.PropertyName,
    //                           BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
    //                       ?? throw new ArgumentException(
    //                           $"Property {filter.PropertyName} does not exist on type {typeof(T).Name}",
    //                           nameof(filter));

    //    var parameter = Expression.Parameter(typeof(T), "x");
    //    var member = Expression.Property(parameter, propertyInfo);
    //    var propertyType = propertyInfo.PropertyType;

    //    object? filterValue = filter.Value;
    //    if (filterValue is not null && propertyType != filterValue.GetType())
    //    {
    //        if (propertyType.IsEnum && filterValue is string enumString)
    //        {
    //            filterValue = Enum.Parse(propertyType, enumString, ignoreCase: true);
    //        }
    //        else
    //        {
    //            filterValue = Convert.ChangeType(filterValue, propertyType);
    //        }
    //    }

    //    var constant = Expression.Constant(filterValue, propertyType);

    //    Expression body = filter.Operator switch
    //    {
    //        FilterOperator.Equals =>
    //            propertyType == typeof(string)
    //                ? BuildStringEqualsCall(member, constant, filter.StringComparison)
    //                : Expression.Equal(member, constant),

    //        FilterOperator.NotEquals =>
    //            propertyType == typeof(string)
    //                ? Expression.Not(BuildStringEqualsCall(member, constant, filter.StringComparison))
    //                : Expression.NotEqual(member, constant),

    //        FilterOperator.Contains =>
    //            BuildStringMethodCall(member, constant, nameof(string.Contains), filter.StringComparison),

    //        FilterOperator.StartsWith =>
    //            BuildStringMethodCall(member, constant, nameof(string.StartsWith), filter.StringComparison),

    //        FilterOperator.EndsWith =>
    //            BuildStringMethodCall(member, constant, nameof(string.EndsWith), filter.StringComparison),

    //        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported")
    //    };

    //    return Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
    //}

    //private static Expression BuildStringEqualsCall(
    //    MemberExpression member,
    //    ConstantExpression constant,
    //    StringComparison stringComparison)
    //{
    //    if (member.Type != typeof(string))
    //    {
    //        throw new NotSupportedException(
    //            "Operator Equals with StringComparison is only supported for string properties.");
    //    }

    //    if (constant.Value is null)
    //    {
    //        throw new ArgumentException("Filter value for operator Equals cannot be null.");
    //    }

    //    var notNull = Expression.NotEqual(
    //        member,
    //        Expression.Constant(null, typeof(string)));

    //    var equalsMethod = typeof(string).GetMethod(
    //                           nameof(string.Equals),
    //                           new[] { typeof(string), typeof(string), typeof(StringComparison) })
    //                       ?? throw new InvalidOperationException(
    //                           "Could not find string.Equals(string, string, StringComparison) overload.");

    //    var call = Expression.Call(
    //        equalsMethod,
    //        member,
    //        constant,
    //        Expression.Constant(stringComparison));

    //    return Expression.AndAlso(notNull, call);
    //}

    //private static Expression BuildStringMethodCall(
    //    MemberExpression member,
    //    ConstantExpression constant,
    //    string methodName,
    //    StringComparison stringComparison)
    //{
    //    if (member.Type != typeof(string))
    //    {
    //        throw new NotSupportedException(
    //            $"Operator {methodName} is only supported for string properties.");
    //    }

    //    if (constant.Value is null)
    //    {
    //        throw new ArgumentException(
    //            $"Filter value for operator {methodName} cannot be null.");
    //    }

    //    var method = typeof(string).GetMethod(
    //                     methodName,
    //                     new[] { typeof(string), typeof(StringComparison) })
    //                 ?? throw new InvalidOperationException(
    //                     $"Could not find string.{methodName}(string, StringComparison) overload.");

    //    var notNull = Expression.NotEqual(
    //        member,
    //        Expression.Constant(null, typeof(string)));

    //    var call = Expression.Call(
    //        member,
    //        method,
    //        constant,
    //        Expression.Constant(stringComparison));

    //    return Expression.AndAlso(notNull, call);
    //}

    //public static Func<T, bool> ConvertFilterItemToFunc<T>(this FilterItem filter)
    //{
    //    ArgumentNullException.ThrowIfNull(filter);

    //    var propertyInfo = typeof(T).GetProperty(
    //        filter.PropertyName,
    //        BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase) ?? throw new ArgumentException($"Property {filter.PropertyName} does not exist on type {typeof(T).Name}", nameof(filter));
    //    var parameter = Expression.Parameter(typeof(T), "x");
    //    var member = Expression.Property(parameter, propertyInfo);
    //    var propertyType = propertyInfo.PropertyType;

    //    object? filterValue = filter.Value;
    //    if (filterValue is not null && propertyType != filterValue.GetType())
    //    {
    //        if (propertyType.IsEnum && filterValue is string enumString)
    //        {
    //            filterValue = Enum.Parse(propertyType, enumString, ignoreCase: true);
    //        }
    //        else
    //        {
    //            filterValue = Convert.ChangeType(filterValue, propertyType);
    //        }
    //    }

    //    var constant = Expression.Constant(filterValue, propertyType);

    //    Expression body = filter.Operator switch
    //    {
    //        FilterOperator.Equals =>
    //            Expression.Equal(member, constant),

    //        FilterOperator.NotEquals =>
    //            Expression.NotEqual(member, constant),

    //        FilterOperator.Contains =>
    //            BuildStringMethodCall(member, constant, nameof(string.Contains)),

    //        FilterOperator.StartsWith =>
    //            BuildStringMethodCall(member, constant, nameof(string.StartsWith)),

    //        FilterOperator.EndsWith =>
    //            BuildStringMethodCall(member, constant, nameof(string.EndsWith)),

    //        _ => throw new NotSupportedException($"Operator {filter.Operator} is not supported")
    //    };

    //    return Expression.Lambda<Func<T, bool>>(body, parameter).Compile();
    //}

    //private static BinaryExpression BuildStringMethodCall(
    //    MemberExpression member,
    //    ConstantExpression constant,
    //    string methodName)
    //{
    //    if (member.Type != typeof(string))
    //    {
    //        throw new NotSupportedException($"Operator {methodName} is only supported for string properties.");
    //    }

    //    if (constant.Value is null)
    //    {
    //        throw new ArgumentException($"Filter value for operator {methodName} cannot be null.");
    //    }

    //    var method = typeof(string).GetMethod(
    //                     methodName,
    //                     new[] { typeof(string), typeof(StringComparison) })
    //                 ?? throw new InvalidOperationException($"Could not find string.{methodName}(string, StringComparison) overload.");

    //    var notNull = Expression.NotEqual(
    //        member,
    //        Expression.Constant(null, typeof(string)));

    //    var call = Expression.Call(
    //        member,
    //        method,
    //        constant,
    //        Expression.Constant(StringComparison.OrdinalIgnoreCase));

    //    return Expression.AndAlso(notNull, call);
    //}
    //private static MethodCallExpression BuildStringMethodCall(
    //    MemberExpression member,
    //    ConstantExpression constant,
    //    string methodName)
    //{
    //    if (member.Type != typeof(string))
    //    {
    //        throw new NotSupportedException(
    //            $"Operator {methodName} is only supported for string properties.");
    //    }

    //    var method = typeof(string).GetMethod(methodName, [typeof(string), typeof(StringComparison)]) ?? throw new InvalidOperationException($"Could not find string.{methodName}(string, StringComparison) overload.");

    //    return Expression.Call(
    //        member,
    //        method,
    //        constant,
    //        Expression.Constant(StringComparison.OrdinalIgnoreCase));
    //}
}
