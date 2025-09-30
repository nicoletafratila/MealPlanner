using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeCategoryService
    {
        Task<RecipeCategoryEditModel?> GetEditAsync(int id); 
        Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters<RecipeCategoryModel>? queryParameters = null);
        Task<CommandResponse?> AddAsync(RecipeCategoryEditModel model);
        Task<CommandResponse?> UpdateAsync(RecipeCategoryEditModel model);
        Task<CommandResponse?> UpdateAsync(IList<RecipeCategoryModel> models);
        Task<CommandResponse?> DeleteAsync(int id);
    }
}
