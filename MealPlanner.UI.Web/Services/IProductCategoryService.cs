using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryEditModel?> GetEditAsync(int id);
        Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<CommandResponse?> AddAsync(ProductCategoryEditModel model);
        Task<CommandResponse?> UpdateAsync(ProductCategoryEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
