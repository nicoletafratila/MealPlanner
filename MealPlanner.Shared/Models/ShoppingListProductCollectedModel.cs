using Common.Models;

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
        public Guid ShoppingListId { get; set; }

        /// <summary>
        /// The product whose collected state is being updated.
        /// </summary>
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
