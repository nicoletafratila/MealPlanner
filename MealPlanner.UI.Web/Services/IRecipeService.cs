using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeService
    {
        Task<IList<RecipeModel>> GetAll();
        Task<EditRecipeModel> Get(int id);
        Task<EditRecipeModel> Add(EditRecipeModel model);
        Task Update(EditRecipeModel model);
    }
}
