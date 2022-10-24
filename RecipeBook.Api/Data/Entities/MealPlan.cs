using Common.Data.Data.Entities;

namespace RecipeBook.Api.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string Name { get; set; }
        public IEnumerable<MealPlanRecipe>? MealPlanRecipes { get; set; }
    }
}
