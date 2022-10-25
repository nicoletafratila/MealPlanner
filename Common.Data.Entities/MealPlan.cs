namespace Common.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string Name { get; set; }
        public IEnumerable<MealPlanRecipe>? MealPlanRecipes { get; set; }
    }
}
