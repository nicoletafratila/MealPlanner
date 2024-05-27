using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
