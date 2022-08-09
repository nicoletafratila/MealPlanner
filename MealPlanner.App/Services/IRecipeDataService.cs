using RecipeBook.Shared.Models;

namespace MealPlanner.App.Services
{
    public interface IRecipeDataService
    {
        Task<IEnumerable<RecipeModel>> GetAll();
        Task<RecipeModel> Get(int id);
        Task<RecipeModel> Add(RecipeModel model);
        Task Update(RecipeModel model);
    }
}
