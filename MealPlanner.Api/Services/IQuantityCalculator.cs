using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Services
{
    public interface IQuantityCalculator
    {
        public IEnumerable<RecipeIngredientModel> CalculateQuantities(IEnumerable<RecipeIngredientModel> ingredients);
    }
}
