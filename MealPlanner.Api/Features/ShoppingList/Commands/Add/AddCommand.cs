using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse?>
    {
        public ShoppingListEditModel? Model { get; set; }
    }
}
