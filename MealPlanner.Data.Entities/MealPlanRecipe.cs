using System.ComponentModel.DataAnnotations.Schema;
using RecipeBook.Data.Entities;

namespace MealPlanner.Data.Entities
{
    public class MealPlanRecipe
    {
        public Guid Id { get; set; }

        [ForeignKey(nameof(MealPlanId))]
        public MealPlan? MealPlan { get; set; }
        public Guid MealPlanId { get; set; }

        [ForeignKey(nameof(RecipeId))]
        public Recipe? Recipe { get; set; }
        public Guid RecipeId { get; set; }

        public override string ToString() =>
            $"Id={Id}, MealPlanId={MealPlanId}, RecipeId={RecipeId}";
    }
}
