using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
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