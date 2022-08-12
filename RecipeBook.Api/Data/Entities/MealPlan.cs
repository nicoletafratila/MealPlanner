namespace RecipeBook.Api.Data.Entities
{
    public class MealPlan : Entity<int>
    {
        public string Name { get; set; }
        //public List<MealPlanRecipe>? Recipes { get; set; }
    }
}
