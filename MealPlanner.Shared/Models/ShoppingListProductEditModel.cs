using System.ComponentModel.DataAnnotations;
using Common.Models;
using MealPlanner.Shared.Resources;
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
        public Guid ShoppingListId { get; set; }

        /// <summary>
        /// Quantity for the product. Must be a positive number (0 or greater).
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ProductQuantityPositive), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Selected unit id for this product.
        /// </summary>
        public Guid UnitId { get; set; }

        /// <summary>
        /// Indicates whether this product has been collected in the shopping list.
        /// </summary>
        public bool Collected { get; set; }

        /// <summary>
        /// Used to control ordering/display of products in the list.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessageResourceName = nameof(MealPlannerSharedMessages.DisplayIndexPositive), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
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
            Guid shoppingListId,
            decimal quantity,
            Guid unitId,
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
