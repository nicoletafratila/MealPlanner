using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Services
{
    public interface IQuantityCalculator
    {
        public IList<RecipeIngredientModel> CalculateQuantities(IList<RecipeIngredientModel> ingredients);
    }
}
