using System.ComponentModel.DataAnnotations;
using Common.Shared;

namespace RecipeBook.Shared.Models
{
    public class RecipeIngredientModel : BaseModel
    {
        public int RecipeId { get; set; }
        public ProductModel? Product { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public decimal Quantity { get; set; }
    }
}
