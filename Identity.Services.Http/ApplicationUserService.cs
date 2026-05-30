using Common.Constants;
using Common.Http;
using Common.Pagination;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Identity.Services.Http
{
    public class ApplicationUserService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ApplicationUserService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            IdentityControllers.ApplicationUserUrl
            ?? throw new ArgumentException("ApplicationUser controller URL is not configured.");

        public async Task<PagedList<ApplicationUserModel>?> SearchAsync(
            QueryParameters<ApplicationUserModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} SearchAsync failed", nameof(ApplicationUserService));
                return null;
            }
        }

        public async Task<ApplicationUserEditModel?> GetEditAsync(string username, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{IdentityControllers.EditRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.Username] = username });
            return await GetAsync<ApplicationUserEditModel>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync failed for model {@Model}", nameof(ApplicationUserService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UnlockAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync<object>(
                    $"{_controller}/{IdentityControllers.UnlockRoute}",
                    new { userId },
                    cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UnlockAsync failed for userId {UserId}", nameof(ApplicationUserService), userId);
                return (false, ex.Message);
            }
        }
    }
}
