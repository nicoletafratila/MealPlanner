using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class RecipeIngredientModel
    {
        public int RecipeId { get; set; }
        public IngredientModel? Ingredient { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public decimal Quantity { get; set; }
    }
}
