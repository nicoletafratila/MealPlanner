using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddShoppingList
{
    public class AddShoppingListCommand : IRequest<AddShoppingListCommandResponse>
    {
        public EditShoppingListModel? Model { get; set; }
    }
}
