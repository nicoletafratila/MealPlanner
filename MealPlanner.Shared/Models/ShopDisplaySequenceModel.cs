using System.ComponentModel.DataAnnotations;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class ShopDisplaySequenceModel
    {
        public int ShopId { get; set; }
        public ProductCategoryModel? ProductCategory { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The display index for the product category must be a positive number.")]
        public int Value { get; set; }
    }
}
