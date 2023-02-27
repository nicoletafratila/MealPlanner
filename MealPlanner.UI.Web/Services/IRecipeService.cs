using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeService
    {
        Task<IList<RecipeModel>> GetAll();
        Task<RecipeModel> Get(int id);
        Task<EditRecipeModel> GetEdit(int id);
        Task<IList<RecipeModel>> Search(int categoryId);
        Task<EditRecipeModel> Add(EditRecipeModel model);
        Task Update(EditRecipeModel model);
        Task DeleteAsync(int id);
    }
}
