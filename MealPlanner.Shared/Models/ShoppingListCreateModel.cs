using Common.Models;

namespace MealPlanner.Shared.Models
{
    /// <summary>
    /// Model for creating a shopping list from a meal plan and a shop.
    /// </summary>
    public class ShoppingListCreateModel : BaseModel
    {
        /// <summary>
        /// The source meal plan id used to generate the shopping list.
        /// </summary>
        public Guid MealPlanId { get; set; }

        /// <summary>
        /// The shop id where this shopping list will be used.
        /// </summary>
        public Guid ShopId { get; set; }

        public ShoppingListCreateModel()
        {
        }

        public ShoppingListCreateModel(Guid mealPlanId, Guid shopId)
        {
            MealPlanId = mealPlanId;
            ShopId = shopId;
        }
    }
}