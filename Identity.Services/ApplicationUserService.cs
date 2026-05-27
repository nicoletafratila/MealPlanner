using System.Net.Http.Json;
using System.Text.Json;
using Common.Api;
using Identity.Shared.Constants;
using Common.Models;
using Common.Pagination;
using Identity.Api;
using Identity.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Identity.Services
{
    public class ApplicationUserService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        IdentityApiConfig identityApiConfig,
        ILogger<ApplicationUserService> logger) : IApplicationUserService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _userController =
            identityApiConfig.Controllers![IdentityControllers.ApplicationUser]
            ?? throw new ArgumentException("ApplicationUser controller URL is not configured.", nameof(identityApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        public async Task<PagedList<ApplicationUserModel>?> SearchAsync(
            QueryParameters<ApplicationUserModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var query = new Dictionary<string, string?>
            {
                [nameof(QueryParameters<>.Filters)] =
                    queryParameters?.Filters is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),

                [nameof(QueryParameters<>.Sorting)] =
                    queryParameters?.Sorting is null
                        ? null
                        : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions),

                [nameof(QueryParameters<>.PageSize)] =
                    (queryParameters?.PageSize ?? int.MaxValue).ToString(),

                [nameof(QueryParameters<>.PageNumber)] =
                    (queryParameters?.PageNumber ?? 1).ToString()
            };

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_userController}/search", query);
            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("ApplicationUser SearchAsync failed with status code {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            try
            {
                return await JsonSerializer.DeserializeAsync<PagedList<ApplicationUserModel>?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize PagedList<ApplicationUserModel>");
                throw;
            }
        }

        public async Task<ApplicationUserEditModel?> GetEditAsync(
            string name,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Username must not be empty.", nameof(name));

            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString(
                $"{_userController}/edit",
                new Dictionary<string, string?>
                {
                    ["username"] = name
                });

            try
            {
                return await httpClient.GetFromJsonAsync<ApplicationUserEditModel?>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize ApplicationUserEditModel for username {UserName}", name);
                throw;
            }
        }

        public async Task<CommandResponse?> UnlockAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId must not be empty.", nameof(userId));

            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PostAsJsonAsync(
                $"{_userController}/unlock",
                new { UserId = userId },
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "ApplicationUser UnlockAsync failed with status code {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    error);

                return CommandResponse.Failed(
                    string.IsNullOrWhiteSpace(error) ? Resources.ApplicationUserServiceMessages.UnlockUserFailed : error);
            }

            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for ApplicationUser UnlockAsync. UserId {UserId}", userId);
                return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.UnlockUserFailed);
            }
        }

        public async Task<CommandResponse?> UpdateAsync(
            ApplicationUserEditModel model,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);

            await EnsureAuthAsync(cancellationToken);

            using var response = await httpClient.PutAsJsonAsync(
                _userController,
                model,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "ApplicationUser UpdateAsync failed with status code {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    error);

                return CommandResponse.Failed(
                    string.IsNullOrWhiteSpace(error) ? Resources.ApplicationUserServiceMessages.UpdateUserFailed : error);
            }

            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<CommandResponse?>(
                    stream,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize CommandResponse for ApplicationUser UpdateAsync. Model {@Model}",
                    model);
                return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.InvalidResponseUserUpdate);
            }
        }
    }
}
