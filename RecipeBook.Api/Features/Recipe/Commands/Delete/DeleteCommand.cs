using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
