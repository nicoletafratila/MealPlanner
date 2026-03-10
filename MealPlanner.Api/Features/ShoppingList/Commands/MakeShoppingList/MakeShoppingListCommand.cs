using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    /// <summary>
    /// Command to generate a shopping list from a meal plan for a given shop.
    /// </summary>
    public class MakeShoppingListCommand : IRequest<ShoppingListEditModel?>
    {
        /// <summary>
        /// Id of the meal plan to generate the shopping list from.
        /// </summary>
        public int MealPlanId { get; set; }

        /// <summary>
        /// Id of the shop for which the shopping list is generated.
        /// </summary>
        public int ShopId { get; set; }

        public MakeShoppingListCommand()
        {
        }

        public MakeShoppingListCommand(int mealPlanId, int shopId)
        {
            MealPlanId = mealPlanId;
            ShopId = shopId;
        }
    }
}