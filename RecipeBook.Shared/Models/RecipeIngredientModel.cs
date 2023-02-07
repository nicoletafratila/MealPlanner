using System.ComponentModel.DataAnnotations;

namespace RecipeBook.Shared.Models
{
    public class RecipeIngredientModel
    {
        public int RecipeId { get; set; }
        public IngredientModel Ingredient { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter the quantity for the ingredient")]
        public decimal Quantity { get; set; }

        public RecipeIngredientModel()
        {
            Ingredient = new IngredientModel();
        }
    }
}
