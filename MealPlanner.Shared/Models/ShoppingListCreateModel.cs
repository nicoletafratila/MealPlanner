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
        public int MealPlanId { get; set; }

        /// <summary>
        /// The shop id where this shopping list will be used.
        /// </summary>
        public int ShopId { get; set; }

        public ShoppingListCreateModel()
        {
        }

        public ShoppingListCreateModel(int mealPlanId, int shopId)
        {
            MealPlanId = mealPlanId;
            ShopId = shopId;
        }
    }
}