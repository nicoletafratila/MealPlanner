using Common.Constants;
using Common.Http;
using Common.Pagination;
using Common.Services;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Core.Http
{
    public class ProductService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<ProductService> logger)
        : ServiceBase(httpClient, tokenProvider)
    {
        private readonly string _controller =
            RecipeBookControllers.ProductUrl
            ?? throw new ArgumentException("Product controller URL is not configured.");

        public async Task<PagedList<ProductModel>?> SearchAsync(
            QueryParameters<ProductModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await SearchAsync(_controller, queryParameters, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} SearchAsync failed", nameof(ProductService));
                return null;
            }
        }

        public async Task<ProductEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(
                $"{_controller}/{RecipeBookControllers.EditRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.Id] = id.ToString() });
            return await GetAsync<ProductEditModel>(url, cancellationToken);
        }

        public async Task<(bool Success, string? Error)> AddAsync(ProductEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PostAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Add failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} AddAsync failed for model {@Model}", nameof(ProductService), model);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(ProductEditModel model, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await PutAsync(_controller, model, cancellationToken);
                return response?.Succeeded == true ? (true, null) : (false, response?.Message ?? "Update failed.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{ServiceName} UpdateAsync failed for model {@Model}", nameof(ProductService), model);
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
                logger.LogError(ex, "{ServiceName} DeleteAsync failed for id {Id}", nameof(ProductService), id);
                return (false, ex.Message);
            }
        }
    }
}
