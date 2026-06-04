namespace MealPlanner.Data.Entities
{
    public class MealPlan : Common.Data.Entities.Entity<Guid>
    {
        public string? UserId { get; set; }

        public string? Name { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public IList<MealPlanRecipe>? MealPlanRecipes { get; set; } = [];

        public MealPlan()
        {
        }
    }
}
