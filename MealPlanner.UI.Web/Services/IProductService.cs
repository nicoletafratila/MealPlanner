using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductService
    {
        Task<ProductEditModel?> GetEditAsync(int id);
        Task<PagedList<ProductModel>?> SearchAsync(QueryParameters<ProductModel>? queryParameters = null);
        Task<CommandResponse?> AddAsync(ProductEditModel model);
        Task<CommandResponse?> UpdateAsync(ProductEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
