using Common.Models;
using Common.Pagination;
using MealPlanner.Shared.Models;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public interface IRecipeService
    {
        Task<RecipeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RecipeEditModel?> GetEditAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IList<ShoppingListProductEditModel>?> GetShoppingListProductsAsync(
            Guid recipeId,
            Guid shopId,
            CancellationToken cancellationToken = default);
        Task<PagedList<RecipeModel>?> SearchAsync(
            QueryParameters<RecipeModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(RecipeEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(RecipeEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
