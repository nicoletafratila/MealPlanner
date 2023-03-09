using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IRecipeCategoryService
    {
        Task<IList<RecipeCategoryModel>> GetAllAsync();
    }
}
