using System.Net.Http.Json;
using System.Text.Json;
using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;

namespace Common.Services
{
    public abstract class ServiceBase(HttpClient httpClient, ITokenProvider tokenProvider)
    {
        protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        protected ITokenProvider TokenProvider { get; } = tokenProvider;

        protected async Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            await httpClient.EnsureAuthorizationHeaderAsync(TokenProvider, cancellationToken);

        protected async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            return await httpClient.GetFromJsonAsync<T>(url, JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> PostAsync<TBody>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<TResponse?> PostAsync<TBody, TResponse>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await httpClient.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> PutAsync<TBody>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await httpClient.PutAsJsonAsync(url, body, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> DeleteAsync(string url, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await httpClient.DeleteAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<PagedList<T>?> SearchAsync<T>(
            string url,
            QueryParameters<T>? queryParameters,
            CancellationToken cancellationToken)
        {
            var query = BuildSearchQuery(queryParameters);
            var fullUrl = BuildUrl($"{url}/{ApiQueryParams.SearchRoute}", query);
            await EnsureAuthAsync(cancellationToken);
            using var response = await httpClient.GetAsync(fullUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<PagedList<T>>(stream, JsonOptions, cancellationToken);
        }

        private static Dictionary<string, string?> BuildSearchQuery<T>(QueryParameters<T>? qp) => new()
        {
            [ApiQueryParams.Filters] = qp?.Filters is null ? null : JsonSerializer.Serialize(qp.Filters, JsonOptions),
            [ApiQueryParams.Sorting] = qp?.Sorting is null || !qp.Sorting.Any() ? null : JsonSerializer.Serialize(qp.Sorting, JsonOptions),
            [ApiQueryParams.PageSize] = (qp?.PageSize ?? int.MaxValue).ToString(),
            [ApiQueryParams.PageNumber] = (qp?.PageNumber ?? 1).ToString()
        };

        protected static string BuildUrl(string baseUrl, IDictionary<string, string?> query)
        {
            var qs = string.Join("&", query
                .Where(kv => kv.Value is not null)
                .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value!)}"));
            return string.IsNullOrEmpty(qs) ? baseUrl : $"{baseUrl}?{qs}";
        }
    }
}
