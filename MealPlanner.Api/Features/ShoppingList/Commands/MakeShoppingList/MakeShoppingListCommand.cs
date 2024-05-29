using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    public class MakeShoppingListCommand : IRequest<ShoppingListEditModel?>
    {
        public int MealPlanId { get; set; }
        public int ShopId { get; set; }
    }
}
