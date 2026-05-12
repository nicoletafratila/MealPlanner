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
        [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.RecipeIdRequired), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public int RecipeId { get; set; }

        /// <summary>
        /// Ingredient quantity. Must be a positive number.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.IngredientQuantityPositive), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Selected unit id for this ingredient.
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessageResourceName = nameof(RecipeBookSharedMessages.IngredientUnitRequired), ErrorMessageResourceType = typeof(RecipeBookSharedMessages))]
        public int UnitId { get; set; }

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

        public RecipeIngredientEditModel(int recipeId, decimal quantity, int unitId)
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