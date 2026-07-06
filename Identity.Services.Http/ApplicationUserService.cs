using System.Text.Json;
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
            try { return await PostAsync($"{_controller}/{IdentityControllers.UnlockRoute}", new { UserId = userId }, cancellationToken); }
            catch (HttpRequestException ex) { logger.LogWarning(ex, "UnlockAsync failed."); return CommandResponse.Failed(ex.Message ?? Resources.ApplicationUserServiceMessages.UnlockUserFailed); }
            catch (JsonException ex) { logger.LogError(ex, "Failed to deserialize CommandResponse for UnlockAsync. UserId {UserId}", userId); return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.UnlockUserFailed); }
        }

        public async Task<CommandResponse?> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(model);
            try { return await PutAsync(_controller, model, cancellationToken); }
            catch (HttpRequestException ex) { logger.LogWarning(ex, "UpdateAsync failed."); return CommandResponse.Failed(ex.Message ?? Resources.ApplicationUserServiceMessages.UpdateUserFailed); }
            catch (JsonException ex) { logger.LogError(ex, "Failed to deserialize CommandResponse for UpdateAsync. Model {@Model}", model); return CommandResponse.Failed(Resources.ApplicationUserServiceMessages.InvalidResponseUserUpdate); }
        }
    }
}
