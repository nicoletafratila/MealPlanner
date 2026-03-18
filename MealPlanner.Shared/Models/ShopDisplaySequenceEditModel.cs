using System.ComponentModel.DataAnnotations;
using Common.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable display sequence entry for a product category in a shop.
    /// </summary>
    public class ShopDisplaySequenceEditModel : BaseModel
    {
        /// <summary>
        /// Parent shop id.
        /// </summary>
        [Required]
        public int ShopId { get; set; }

        /// <summary>
        /// Display index for the product category (0 or greater).
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The display index for the product category must be a positive number.")]
        public int Value { get; set; }

        /// <summary>
        /// Linked product category.
        /// </summary>
        public ProductCategoryModel? ProductCategory { get; set; }

        public ShopDisplaySequenceEditModel()
        {
        }

        public ShopDisplaySequenceEditModel(int shopId, int value, ProductCategoryModel? category)
        {
            ShopId = shopId;
            Value = value;
            ProductCategory = category;
        }

        public override string ToString()
            => ProductCategory?.Name ?? $"Category {Value}";
    }
}