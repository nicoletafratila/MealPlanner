using System.Text.Json;
using Common.Pagination;

namespace Common.Services
{
    public static class SearchCacheKeyBuilder
    {
        public static string Build<T>(string prefix, QueryParameters<T>? queryParameters, string? userId, bool thumbnailOnly = false)
        {
            var sorting = queryParameters?.Sorting is null
                ? null
                : JsonSerializer.Serialize(queryParameters.Sorting);
            var filters = queryParameters?.Filters is null
                ? null
                : JsonSerializer.Serialize(queryParameters.Filters);

            return $"{prefix}:{userId}:{queryParameters?.PageNumber}:{queryParameters?.PageSize}:{sorting}:{filters}:{thumbnailOnly}";
        }
    }
}
