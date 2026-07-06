using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class ProductCategoryService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ProductCategoryService> logger)
        : ServiceBase(httpClient, tokenProvider), IProductCategoryService
    {
        private readonly string _controller = RecipeBookControllers.ProductCategoryUrl;

        public async Task<ProductCategoryEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{RecipeBookControllers.EditRoute}", new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ProductCategoryEditModel>(url, cancellationToken);
        }

        public Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters<ProductCategoryModel>? queryParameters = null, CancellationToken cancellationToken = default)
            => SearchAsync(_controller, queryParameters, cancellationToken);

        public async Task<CommandResponse?> AddAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PostAsync(_controller, model, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProductCategory AddAsync failed. Model {@Model}", model); throw;
            }
        }

        public async Task<CommandResponse?> UpdateAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                return await PutAsync(_controller, model, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProductCategory UpdateAsync failed. Model {@Model}", model); throw;
            }
        }

        public async Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(_controller, new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            try
            {
                return await DeleteAsync(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ProductCategory DeleteAsync failed. Id {Id}", id); throw;
            }
        }
    }
}
