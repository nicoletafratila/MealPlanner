using System.Net.Http.Json;using Common.Models; using Common.Pagination; using Microsoft.Extensions.Logging; using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Core
{
    public interface IProductService
    {
        Task<ProductEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedList<ProductModel>?> SearchAsync(
            QueryParameters<ProductModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(ProductEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ProductEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
