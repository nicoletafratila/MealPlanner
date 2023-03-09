using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IIngredientCategoryService
    {
        Task<IList<IngredientCategoryModel>> GetAllAsync();
    }
}
