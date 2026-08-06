using System.Text.Json;
using Common.Pagination;

namespace Common.Services
{
    public static class SearchCacheKeyBuilder
    {
        public static string Build<T>(string prefix, QueryParameters<T>? queryParameters)
        {
            var sorting = queryParameters?.Sorting is null
                ? null
                : JsonSerializer.Serialize(queryParameters.Sorting);
            var filters = queryParameters?.Filters is null
                ? null
                : JsonSerializer.Serialize(queryParameters.Filters);

            return $"{prefix}:{queryParameters?.PageNumber}:{queryParameters?.PageSize}:{sorting}:{filters}";
        }
    }
}
