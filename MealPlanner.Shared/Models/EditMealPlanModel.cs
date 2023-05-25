using Common.Validators;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class EditMealPlanModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [MinimumCountCollection(1, ErrorMessage = "The meal plan requires at least one recipe.")]
        public IList<RecipeModel>? Recipes { get; set; }
    }
}
