using Common.Shared;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class ShoppingListProductEditModel : BaseModel
    {
        [Required]
        public int ShoppingListId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public decimal Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the product.")]
        public int UnitId { get; set; }

        public bool Collected { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The display index for the product category must be a positive number.")]
        public int DisplaySequence { get; set; }

        public UnitModel? Unit { get; set; }

        public ProductModel? Product { get; set; }
    }
}
