using System.ComponentModel.DataAnnotations;
using Common.Models;

namespace RecipeBook.Shared.Models
{
    public class RecipeIngredientEditModel : BaseModel
    {
        [Required]
        public int RecipeId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the ingredient must be a positive number.")]
        public double Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the ingredient.")]
        public int UnitId { get; set; }

        public UnitModel? Unit { get; set; }

        public ProductModel? Product { get; set; }
    }
}
