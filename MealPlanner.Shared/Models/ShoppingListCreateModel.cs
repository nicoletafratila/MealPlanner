using Common.Models;
using Common.Validators;
using MealPlanner.Shared.Resources;

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
        [RequiredGuid(ErrorMessageResourceName = nameof(MealPlannerSharedMessages.MealPlanRequired), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
        public Guid MealPlanId { get; set; }

        /// <summary>
        /// The shop id where this shopping list will be used.
        /// </summary>
        [RequiredGuid(ErrorMessageResourceName = nameof(MealPlannerSharedMessages.ShopRequired), ErrorMessageResourceType = typeof(MealPlannerSharedMessages))]
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