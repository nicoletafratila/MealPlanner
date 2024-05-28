using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeCategoryService
    {
        Task<EditRecipeCategoryModel?> GetEditAsync(int id); 
        Task<IList<RecipeCategoryModel>?> GetAllAsync();
        Task<PagedList<RecipeCategoryModel>?> SearchAsync(QueryParameters? queryParameters = null);
        Task<string?> AddAsync(EditRecipeCategoryModel model);
        Task<string?> UpdateAsync(EditRecipeCategoryModel model);
        Task<string?> UpdateAsync(IList<RecipeCategoryModel> models);
        Task<string?> DeleteAsync(int id);
    }
}
