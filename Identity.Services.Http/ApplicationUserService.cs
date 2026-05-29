using Common.Http;
using Common.Pagination;
using Common.Services;
using Identity.Shared.Constants;
using Identity.Shared.Models;

namespace Identity.Services.Http
{
    public class ApplicationUserService(HttpClient httpClient, ITokenProvider tokenProvider)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            IdentityControllers.ApplicationUserUrl
            ?? throw new ArgumentException("ApplicationUser controller URL is not configured.");

        public Task<PagedList<ApplicationUserModel>?> SearchAsync(
            QueryParameters<ApplicationUserModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<ApplicationUserEditModel?> GetEditAsync(string username, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/edit", new Dictionary<string, string?> { ["username"] = username });
            return await GetAsync<ApplicationUserEditModel>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(ApplicationUserEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string? Error)> UnlockAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync<object>($"{_controller}/unlock", new { userId }, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Failed.");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }
    }
}
