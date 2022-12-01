using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Data.Entities
{
    public class MealPlanRecipe
    {
        [ForeignKey("MealPlanId")]
        public MealPlan MealPlan { get; private set; }
        public int MealPlanId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe Recipe { get; private set; }
        public int RecipeId { get; set; }
    }
}
