using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Services
{
    public class QuantityCalculator : IQuantityCalculator
    {
        public IEnumerable<RecipeIngredientModel> CalculateQuantities(IEnumerable<RecipeIngredientModel> ingredients)
        {
            foreach (var item in ingredients)
            {
                item.Quantity = ingredients.Where(i => i.Id == item.Id).Sum(i => i.Quantity);
            }
            return ingredients.DistinctBy(i => i.Id).OrderBy(i => i.DisplaySequence);
        }
    }
}
