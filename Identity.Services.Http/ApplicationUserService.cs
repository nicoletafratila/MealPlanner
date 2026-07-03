using System.Net.Http.Json;
using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services.Http
{
    public class ApplicationUserService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ApplicationUserService> logger)
        : ServiceBase(httpClient, tokenProvider), IApplicationUserService
    {
        private readonly string _controller = IdentityControllers.ApplicationUserUrl;

        public Task<PagedList<ApplicationUserModel>?> SearchAsync(QueryParameters<ApplicationUserModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<ApplicationUserEditModel?> GetEditAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Username must not be empty.", nameof(name));
            var url = BuildUrl($"{_controller}/{IdentityControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Username] = name });
            return await GetAsync<ApplicationUserEditModel>(url, cancellationToken);
        }

        public async Task<CommandResponse?> UnlockAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId must not be empty.", nameof(userId));
            try
            {
                using var response = await HttpClient.PostAsJsonAsync($"{_controller}/{IdentityControllers.UnlockRoute}", new { UserId = userId }, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("UnlockAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.ApplicationUserServiceMessages.UnlockUserFailed : error);
                }
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await System.Text.Json.JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions, cancellationToken);
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for UnlockAsync. UserId {UserId}", userId);
                return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.UnlockUserFailed);
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            try
            {
                using var response = await HttpClient.PutAsJsonAsync(_controller, model, JsonOptions, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);
                    logger.LogWarning("UpdateAsync failed with status {StatusCode}.", response.StatusCode);
                    return CommandResponse.Failed(string.IsNullOrWhiteSpace(error) ? Resources.ApplicationUserServiceMessages.UpdateUserFailed : error);
                }
                await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                return await System.Text.Json.JsonSerializer.DeserializeAsync<CommandResponse?>(stream, JsonOptions, cancellationToken);
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize CommandResponse for UpdateAsync. Model {@Model}", model);
                return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.InvalidResponseUserUpdate);
            }
        }
    }
}
