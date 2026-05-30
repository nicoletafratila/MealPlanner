using System.Net.Http.Json;using Common.Constants; using Common.Http; using Common.Models; using Common.Pagination; using Common.Services; using MealPlanner.Shared.Constants; using MealPlanner.Shared.Models; using Microsoft.Extensions.Logging;

namespace MealPlanner.Services.Http
{
    public class ShoppingListService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ShoppingListService> logger)
        : ServiceBase(httpClient, tokenProvider), IShoppingListService
    {
        private readonly string _controller = MealPlannerControllers.ShoppingListUrl;

        public async Task<ShoppingListEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ShoppingListEditModel>(url, cancellationToken);
        }

        public Task<PagedList<ShoppingListModel>?> SearchAsync(QueryParameters<ShoppingListModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<ShoppingListEditModel?> MakeShoppingListAsync(ShoppingListCreateModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync<ShoppingListCreateModel, ShoppingListEditModel>($"{_controller}/{MealPlannerControllers.MakeShoppingListRoute}", model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "MakeShoppingListAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> AddAsync(ShoppingListEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "ShoppingList AddAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(ShoppingListEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PutAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "ShoppingList UpdateAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { return await DeleteAsync(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "ShoppingList DeleteAsync failed. Id {Id}", id); throw; }
        }
    }
}
