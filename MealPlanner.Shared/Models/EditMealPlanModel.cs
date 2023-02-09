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
        public string Name { get; set; }

        public IList<RecipeModel>? Recipes { get; set; }

        public EditMealPlanModel()
        {
            Recipes = new List<RecipeModel>();
        }
    }
}
