using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IIngredientService
    {
        Task<IList<IngredientModel>> GetAll();
        Task<IList<IngredientModel>> Search(int categoryId);
        Task<EditIngredientModel> Get(int id);
        Task<EditIngredientModel> Add(EditIngredientModel model);
        Task Update(EditIngredientModel model);
    }
}
