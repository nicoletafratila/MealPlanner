namespace Common.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string? Name { get; set; }
        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; }
    }
}
