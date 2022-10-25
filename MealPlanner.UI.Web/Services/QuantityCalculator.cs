using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class QuantityCalculator : IQuantityCalculator
    {
        public IEnumerable<IngredientModel> CalculateQuantities(IEnumerable<IngredientModel> ingredients)
        {
            return ingredients
                 .GroupBy(l => l.Id)
                 .Select(cl => new IngredientModel
                 {
                     Id = cl.First().Id,
                     RecipeId = cl.First().Id,
                     Name = cl.First().Name,
                     Unit = cl.First().Unit,
                     Quantity = cl.Sum(q => q.Quantity),
                 }).ToList();
        }
    }
}
