using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
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