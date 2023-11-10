using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateShoppingList
{
    public class UpdateShoppingListCommand : IRequest<UpdateShoppingListCommandResponse>
    {
        public EditShoppingListModel? Model { get; set; }
    }
}
