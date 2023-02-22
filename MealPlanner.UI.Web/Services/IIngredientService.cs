using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IIngredientService
    {
        Task<IList<IngredientModel>> GetAllAsync();
        Task<EditIngredientModel> GetAsync(int id);
        Task<IList<IngredientModel>> SearchAsync(int categoryId);
        Task<EditIngredientModel> AddAsync(EditIngredientModel model);
        Task UpdateAsync(EditIngredientModel model);
        Task DeleteAsync(int id);
    }
}
