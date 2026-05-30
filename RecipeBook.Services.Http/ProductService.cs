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
    public class ProductService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ProductService> logger)
        : ServiceBase(httpClient, tokenProvider), IProductService
    {
        private readonly string _controller = RecipeBookControllers.ProductUrl;

        public async Task<ProductEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ProductEditModel>(url, cancellationToken);
        }

        public Task<PagedList<ProductModel>?> SearchAsync(QueryParameters<ProductModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(ProductEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PostAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Product AddAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> UpdateAsync(ProductEditModel model, CancellationToken cancellationToken = default)
        {
            try { return await PutAsync(_controller, model, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Product UpdateAsync failed. Model {@Model}", model); throw; }
        }

        public async Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try { return await DeleteAsync(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "Product DeleteAsync failed. Id {Id}", id); throw; }
        }
    }
}
