using MediatR;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    public class DeleteCommand : IRequest<DeleteCommandResponse>
    {
        public int Id { get; set; }
    }
}
