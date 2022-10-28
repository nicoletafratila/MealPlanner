using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Services
{
    public interface IQuantityCalculator
    {
        public IEnumerable<IngredientModel> CalculateQuantities(IEnumerable<IngredientModel> ingredients);
    }
}
