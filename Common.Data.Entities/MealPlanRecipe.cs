using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class MealPlanRecipe
    {
        public int MealPlanId { get; set; }
        public int RecipeId { get; set; }

        [ForeignKey("MealPlanId")]
        public MealPlan MealPlan { get; private set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; private set; }
    }
}
