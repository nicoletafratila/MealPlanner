using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductCategoryService
    {
        Task<ProductCategoryEditModel?> GetEditAsync(int id);
        Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(ProductCategoryEditModel model);
        Task<string?> UpdateAsync(ProductCategoryEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}
