using Common.Constants;
using Common.Http;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Core.Http
{
    public class RecipeCategoryService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<RecipeCategoryService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            RecipeBookControllers.RecipeCategoryUrl
            ?? throw new ArgumentException("RecipeCategory controller URL is not configured.");

        public async Task<PagedList<RecipeCategoryModel>?> SearchAsync(
            QueryParameters<RecipeCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} SearchAsync failed", nameof(RecipeCategoryService));
                return null;
            }
        }

        public async Task<RecipeCategoryEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{RecipeBookControllers.EditRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<RecipeCategoryEditModel>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AddAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Add failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} AddAsync failed for model {@Model}", nameof(RecipeCategoryService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync failed for model {@Model}", nameof(RecipeCategoryService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(IList<RecipeCategoryModel> models, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync($"{_controller}/{RecipeBookControllers.UpdateAllRoute}", models, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync (batch) failed", nameof(RecipeCategoryService));
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
                var response = await DeleteAsync(url, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Delete failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} DeleteAsync failed for id {Id}", nameof(RecipeCategoryService), id);
                return (false, ex.Message);
            }
        }
    }
}
