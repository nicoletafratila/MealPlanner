using System.Linq.Expressions;
using System.Reflection;

namespace Common.Pagination
{
    /// <summary>
    /// Resolves dotted property paths (e.g. "RecipeCategory.Name") into a chained
    /// <see cref="MemberExpression"/>, so filtering/sorting can reach navigation properties
    /// on the entity being queried, not just its own direct properties.
    /// </summary>
    internal static class PropertyPathExpression
    {
        public static MemberExpression Resolve(Expression instance, string propertyPath)
        {
            Expression current = instance;

            foreach (var segment in propertyPath.Split('.'))
            {
                var propertyInfo = current.Type.GetProperty(
                        segment,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase)
                    ?? throw new ArgumentException(
                        $"Property {segment} does not exist on type {current.Type.Name}");

                current = Expression.Property(current, propertyInfo);
            }

            return (MemberExpression)current;
        }
    }
}
