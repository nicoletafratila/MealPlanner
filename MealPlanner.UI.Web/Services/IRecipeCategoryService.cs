using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeCategoryService
    {
        Task<IList<RecipeCategoryModel>?> GetAllAsync();
        Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
    }
}
