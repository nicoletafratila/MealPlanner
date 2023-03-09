using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeService
    {
        Task<IList<RecipeModel>> GetAllAsync();
        Task<RecipeModel> GetByIdAsync(int id);
        Task<EditRecipeModel> GetEditAsync(int id);
        Task<IList<RecipeModel>> SearchAsync(int categoryId);
        Task<EditRecipeModel> AddAsync(EditRecipeModel model);
        Task UpdateAsync(EditRecipeModel model);
        Task DeleteAsync(int id);
    }
}
