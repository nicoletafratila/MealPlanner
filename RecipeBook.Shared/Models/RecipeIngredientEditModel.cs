using System.ComponentModel.DataAnnotations;
using Common.Models;
using RecipeBook.Shared.Resources;

namespace RecipeBook.Shared.Models
{
    /// <summary>
    /// Editable ingredient line for a recipe.
    /// </summary>
    public class RecipeIngredientEditModel : BaseModel
    {
        /// <summary>
        /// The parent recipe id this ingredient belongs to.
        /// </summary>
        [Required]
        public Guid RecipeId { get; set; }

        /// <summary>
        /// Ingredient quantity. Must be a positive number.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.IngredientQuantityPositive), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Selected product id for this ingredient.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Selected unit id for this ingredient.
        /// </summary>
        public Guid UnitId { get; set; }

        /// <summary>
        /// Selected unit details (optional, for display/UI).
        /// </summary>
        public UnitModel? Unit { get; set; }

        /// <summary>
        /// Linked product for this ingredient (optional, for display/UI).
        /// </summary>
        public ProductModel? Product { get; set; }

        public RecipeIngredientEditModel()
        {
        }

        public RecipeIngredientEditModel(Guid recipeId, decimal quantity, Guid unitId)
        {
            RecipeId = recipeId;
            Quantity = quantity;
            UnitId = unitId;
        }

        public override string ToString()
        {
            var productName = Product?.Name ?? string.Empty;
            var unitName = Unit?.Name ?? string.Empty;
            return $"{Quantity} {unitName} {productName}".Trim();
        }
    }
}
