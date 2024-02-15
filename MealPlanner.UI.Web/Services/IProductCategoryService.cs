using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IProductCategoryService
    {
        Task<IList<ProductCategoryModel>?> GetAllAsync();
        Task<PagedList<ProductCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
    }
}
