using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddMealPlanToShoppingList
{
    public class AddMealPlanToShoppingListCommand : IRequest<EditShoppingListModel?>
    {
        public int MealPlanId { get; set; }
        public int ShoppingListId { get; set; }
    }
}
