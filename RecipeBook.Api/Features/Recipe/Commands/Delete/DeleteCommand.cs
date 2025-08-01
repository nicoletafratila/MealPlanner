using Common.Models;
using MediatR;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommand : IRequest<CommandResponse>
    {
        public int Id { get; set; }
    }
}
