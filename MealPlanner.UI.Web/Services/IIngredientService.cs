using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientModel>> GetAll();
        Task<EditIngredientModel> Get(int id);
        Task<EditIngredientModel> Add(EditIngredientModel model);
        Task Update(EditIngredientModel model);
    }
}
