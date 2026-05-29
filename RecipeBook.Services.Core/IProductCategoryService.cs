using System.Net.Http.Json;using Common.Models; using Common.Pagination; using Microsoft.Extensions.Logging; using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Core
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedList<ProductCategoryModel>?> SearchAsync(
            QueryParameters<ProductCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(ProductCategoryEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}
