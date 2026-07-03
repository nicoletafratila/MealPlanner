using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Models;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class RecipeService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<RecipeService> logger)
        : ServiceBase(httpClient, tokenProvider), IRecipeService
    {
        private readonly string _controller = RecipeBookControllers.RecipeUrl;

        public async Task<RecipeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { return await GetAsync<RecipeModel>(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Failed to fetch RecipeModel for id {RecipeId}", id); return null; }
        }

        public async Task<RecipeEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<RecipeEditModel>(url, cancellationToken);
        }

        public async Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(Guid recipeId, Guid shopId, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.ShoppingListProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.RecipeId] = recipeId.ToString(), [ApiQueryParams.ShopId] = shopId.ToString() });
            return await GetAsync<IList<ShoppingListProductEditModel>>(url, cancellationToken);
        }

        public Task<PagedList<RecipeModel>?> SearchAsync(QueryParameters<RecipeModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(RecipeEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Recipe AddAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(RecipeEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PutAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Recipe UpdateAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { return await DeleteAsync(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Recipe DeleteAsync failed. Id {Id}", id); throw; }
        }
    }
}
