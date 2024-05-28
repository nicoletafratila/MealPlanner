using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
