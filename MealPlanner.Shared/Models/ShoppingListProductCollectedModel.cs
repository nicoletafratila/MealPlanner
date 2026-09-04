using Common.Models;
using Common.Validators;
using MealPlanner.Shared.Resources;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Lightweight payload for toggling a single shopping list product's collected state.
    /// </summary>
    public class ShoppingListProductCollectedModel : BaseModel
    {
        /// <summary>
        /// Parent shopping list id.
        /// </summary>
        [RequiredGuid(ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ShoppingListRequired), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public Guid ShoppingListId { get; set; }

        /// <summary>
        /// The product whose collected state is being updated.
        /// </summary>
        [RequiredGuid(ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ProductRequired), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Whether the product has been collected.
        /// </summary>
        public bool Collected { get; set; }

        public ShoppingListProductCollectedModel()
        {
        }

        public ShoppingListProductCollectedModel(Guid shoppingListId, Guid productId, bool collected)
        {
            ShoppingListId = shoppingListId;
            ProductId = productId;
            Collected = collected;
        }
    }
}
