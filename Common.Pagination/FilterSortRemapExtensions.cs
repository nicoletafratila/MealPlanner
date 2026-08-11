namespace Common.Pagination
{
    /// <summary>
    /// Lets a repository translate a DTO-facing flat property name (e.g. "RecipeCategoryName") into the
    /// entity's navigation path (e.g. "RecipeCategory.Name") before running <see cref="FilterExtensions.ApplyFilters{T}"/>
    /// / <see cref="EnumerableExtensions.ApplySorting{TItem}"/> against the entity, without requiring the
    /// client (which reflects against the DTO type to build its filter UI) to know about the entity shape.
    /// </summary>
    public static class FilterSortRemapExtensions
    {
        public static IEnumerable<FilterItem>? RemapPropertyName(this IEnumerable<FilterItem>? filters, string from, string to)
        {
            if (filters is null)
                return null;

            return filters.Select(f =>
                string.Equals(f.PropertyName, from, StringComparison.OrdinalIgnoreCase)
                    ? new FilterItem(to, f.Value, f.Operator, f.StringComparison)
                    : f);
        }

        public static IEnumerable<SortingModel>? RemapPropertyName(this IEnumerable<SortingModel>? sorting, string from, string to)
        {
            if (sorting is null)
                return null;

            return sorting.Select(s =>
                string.Equals(s.PropertyName, from, StringComparison.OrdinalIgnoreCase)
                    ? new SortingModel { PropertyName = to, Direction = s.Direction }
                    : s);
        }

        /// <summary>
        /// Pulls a boolean sentinel out of a <see cref="FilterItem"/> list sent by the client (e.g. a
        /// "ThumbnailOnly" flag that isn't a real entity property and would otherwise crash <see cref="FilterExtensions.ApplyFilters{T}"/>),
        /// returning whether it was present and true, plus the remaining filters with that entry removed.
        /// When no matching entry exists, <paramref name="remaining"/> is the original <paramref name="filters"/>
        /// reference unchanged (including null), so callers with no sentinel see no behavior change.
        /// </summary>
        public static bool TryExtractBooleanFlag(this IEnumerable<FilterItem>? filters, string propertyName, out IEnumerable<FilterItem>? remaining)
        {
            if (filters is null)
            {
                remaining = null;
                return false;
            }

            var list = filters.ToList();
            var match = list.FirstOrDefault(f => string.Equals(f.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                remaining = filters;
                return false;
            }

            remaining = list.Where(f => f != match).ToList();
            return match.Value is true;
        }
    }
}
