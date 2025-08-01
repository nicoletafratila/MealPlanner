using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    public class DeleteCommand : IRequest<CommandResponse>
    {
        public int Id { get; set; }
    }
}
