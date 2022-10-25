using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public interface IQuantityCalculator
    {
        public IEnumerable<IngredientModel> CalculateQuantities(IEnumerable<IngredientModel> ingredients);
    }
}
