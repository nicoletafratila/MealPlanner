using RecipeBook.Shared.Models;

namespace MealPlanner.App.Services
{
    public interface IQuantityCalculator
    {
        public IEnumerable<IngredientModel> CalculateQuantities(IEnumerable<IngredientModel> ingredients);
    }
}
