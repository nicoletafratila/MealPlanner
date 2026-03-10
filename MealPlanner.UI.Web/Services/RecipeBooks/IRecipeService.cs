using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public interface IRecipeService
    {
        Task<RecipeModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<RecipeEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);
        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(
            int recipeId,
            int shopId,
            CancellationToken cancellationToken = default);
        Task<PagedList<RecipeModel>?> SearchAsync(
            QueryParameters<RecipeModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(RecipeEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(RecipeEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}