using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class ShoppingListModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public IList<RecipeIngredientModel>? Ingredients { get; set; }
    }
}
