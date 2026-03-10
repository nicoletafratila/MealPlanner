using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
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