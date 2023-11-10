using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList
{
    public class AddShoppingListCommand : IRequest<EditShoppingListModel?>
    {
        public int MealPlanId { get; set; }
    }
}
