using Common.Pagination;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeService
    {
        Task<RecipeModel?> GetByIdAsync(int id);
        Task<RecipeEditModel?> GetEditAsync(int id);
        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(int recipeId, int shopId);
        Task<PagedList<RecipeModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(RecipeEditModel model);
        Task<string?> UpdateAsync(RecipeEditModel model);
        Task<string?> DeleteAsync(int id);
    }
}