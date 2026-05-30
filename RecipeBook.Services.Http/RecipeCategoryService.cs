using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;
using Microsoft.Extensions.Logging;

namespace RecipeBook.Services.Http
{
    public class RecipeCategoryService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<RecipeCategoryService> logger)
        : ServiceBase(httpClient, tokenProvider), IRecipeCategoryService
    {
        private readonly string _controller = RecipeBookControllers.RecipeCategoryUrl;

        public async Task<RecipeCategoryEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<RecipeCategoryEditModel>(url, cancellationToken);
        }

        public Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters<RecipeCategoryModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "RecipeCategory AddAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PutAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "RecipeCategory UpdateAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(IList<RecipeCategoryModel> models, CancellationToken cancellationToken = default)
        {
            try { return await PutAsync($"{_controller}/{RecipeBookControllers.UpdateAllRoute}", models, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "RecipeCategory bulk UpdateAsync failed. Models {@Models}", models); throw; }
        }

        public async Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { return await DeleteAsync(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "RecipeCategory DeleteAsync failed. Id {Id}", id); throw; }
        }
    }
}
