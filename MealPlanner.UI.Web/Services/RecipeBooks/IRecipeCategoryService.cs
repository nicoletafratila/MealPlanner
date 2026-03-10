using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.RecipeBooks
{
    public interface IRecipeCategoryService
    {
        Task<RecipeCategoryEditModel?> GetEditAsync(int id, CancellationToken cancellationToken = default);
        Task<PagedList<RecipeCategoryModel>?> SearchAsync(
            QueryParameters<RecipeCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> AddAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(RecipeCategoryEditModel model, CancellationToken cancellationToken = default);
        Task<CommandResponse?> UpdateAsync(
            IList<RecipeCategoryModel> models,
            CancellationToken cancellationToken = default);
        Task<CommandResponse?> DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}