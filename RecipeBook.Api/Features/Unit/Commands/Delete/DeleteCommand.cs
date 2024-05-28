using MediatR;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
