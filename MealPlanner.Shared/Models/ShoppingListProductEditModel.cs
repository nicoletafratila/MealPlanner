using System.ComponentModel.DataAnnotations;
using Common.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Editable line item in a shopping list.
    /// </summary>
    public class ShoppingListProductEditModel : BaseModel
    {
        /// <summary>
        /// Parent shopping list id.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a shopping list for the product.")]
        public int ShoppingListId { get; set; }

        /// <summary>
        /// Quantity for the product. Must be a positive number (0 or greater).
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "The quantity for the product must be a positive number.")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Selected unit id for this product.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a unit of measurement for the product.")]
        public int UnitId { get; set; }

        /// <summary>
        /// Indicates whether this product has been collected in the shopping list.
        /// </summary>
        public bool Collected { get; set; }

        /// <summary>
        /// Used to control ordering/display of products in the list.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The display index for the product category must be a positive number.")]
        public int DisplaySequence { get; set; }

        /// <summary>
        /// Unit details (for display).
        /// </summary>
        public UnitModel? Unit { get; set; }

        /// <summary>
        /// Product details (for display).
        /// </summary>
        public ProductModel? Product { get; set; }

        public ShoppingListProductEditModel()
        {
        }

        public ShoppingListProductEditModel(
            int shoppingListId,
            decimal quantity,
            int unitId,
            int displaySequence)
        {
            ShoppingListId = shoppingListId;
            Quantity = quantity;
            UnitId = unitId;
            DisplaySequence = displaySequence;
        }

        public override string ToString()
        {
            var productName = Product?.Name ?? string.Empty;
            var unitName = Unit?.Name ?? string.Empty;
            var core = $"{Quantity} {unitName} {productName}".Trim();
            return string.IsNullOrEmpty(core) ? Quantity.ToString() : core;
        }
    }
}