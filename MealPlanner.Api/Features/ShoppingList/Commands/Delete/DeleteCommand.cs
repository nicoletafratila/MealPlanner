using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
