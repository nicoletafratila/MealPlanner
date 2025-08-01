using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse>
    {
        public ShoppingListEditModel? Model { get; set; }
    }
}
