using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeCategoryService
    {
        Task<RecipeCategoryEditModel?> GetEditAsync(int id); 
        Task<IList<RecipeCategoryModel>?> GetAllAsync();
        Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(RecipeCategoryEditModel model);
        Task<string?> UpdateAsync(RecipeCategoryEditModel model);
        Task<string?> UpdateAsync(IList<RecipeCategoryModel> models);
        Task<string?> DeleteAsync(int id);
    }
}
