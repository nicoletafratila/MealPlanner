using System.ComponentModel.DataAnnotations;
using Common.Shared;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    public class ShopDisplaySequenceModel : BaseModel
    {
        public int ShopId { get; set; }
        public ProductCategoryModel? ProductCategory { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The display index for the product category must be a positive number.")]
        public int Value { get; set; }
    }
}
