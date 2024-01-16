using Common.Shared;
using RecipeBook.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MealPlanner.Shared.Models
{
    public class ShoppingListProductModel : BaseModel
    {
        public int ShoppingListId { get; set; }
        public bool Collected { get; set; }
        public int DisplaySequence { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public decimal Quantity { get; set; }

        public ProductModel? Product { get; set; }
    }
}
