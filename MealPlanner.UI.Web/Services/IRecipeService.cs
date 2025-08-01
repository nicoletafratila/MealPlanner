using Common.Models;
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
        Task<CommandResponse?> AddAsync(RecipeEditModel model);
        Task<CommandResponse?> UpdateAsync(RecipeEditModel model);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}