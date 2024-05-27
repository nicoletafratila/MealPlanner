using MediatR;

namespace MealPlanner.Api.Features.ProductCategory.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
