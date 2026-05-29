using System.ComponentModel.DataAnnotations.Schema;using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities
{
    public sealed class MealPlanRecipe
    {
        [ForeignKey(nameof(MealPlanId))]
        public MealPlan? MealPlan { get; set; }
        public int MealPlanId { get; set; }

        [ForeignKey(nameof(RecipeId))]
        public Recipe? Recipe { get; set; }
        public int RecipeId { get; set; }

        public override string ToString() =>
            $"MealPlanId={MealPlanId}, RecipeId={RecipeId}";
    }
}
