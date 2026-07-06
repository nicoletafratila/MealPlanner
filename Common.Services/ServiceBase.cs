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

        protected HttpClient HttpClient { get; } = httpClient;

        protected ITokenProvider TokenProvider { get; } = tokenProvider;

        protected async Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            await HttpClient.EnsureAuthorizationHeaderAsync(TokenProvider, cancellationToken);

        protected async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await HttpClient.GetAsync(url, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> PostAsync<TBody>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await HttpClient.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<TResponse?> PostAsync<TBody, TResponse>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await HttpClient.PostAsJsonAsync(url, body, JsonOptions, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> PutAsync<TBody>(string url, TBody body, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await HttpClient.PutAsJsonAsync(url, body, JsonOptions, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadFromJsonAsync<CommandResponse>(JsonOptions, cancellationToken);
        }

        protected async Task<CommandResponse?> DeleteAsync(string url, CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);
            using var response = await HttpClient.DeleteAsync(url, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
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
            using var response = await HttpClient.GetAsync(fullUrl, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<PagedList<T>>(stream, JsonOptions, cancellationToken);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode) return;
            var message = await ReadErrorMessageAsync(response, cancellationToken)
                ?? $"Request failed with status {(int)response.StatusCode}.";
            throw new HttpRequestException(message, null, response.StatusCode);
        }

        protected static async Task<string?> ReadErrorMessageAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body)) return null;
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                    return detail.GetString();
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                            ? p.Value.EnumerateArray()
                                .Select(v => v.GetString())
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                            : [])
                        .ToList();
                    if (messages.Count > 0) return string.Join(Environment.NewLine, messages);
                }
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString();
            }
            catch (JsonException) { }
            return body;
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
