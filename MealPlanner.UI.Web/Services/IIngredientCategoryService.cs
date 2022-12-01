using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IIngredientCategoryService
    {
        Task<IEnumerable<IngredientCategoryModel>> GetAll();
    }
}
