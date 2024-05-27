using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductCategoryService
    {
        Task<EditProductCategoryModel?> GetEditAsync(int id);
        Task<IList<ProductCategoryModel>?> GetAllAsync();
        Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(EditProductCategoryModel model);
        Task<string?> UpdateAsync(EditProductCategoryModel model);
        Task<string?> DeleteAsync(int id);
    }
}
