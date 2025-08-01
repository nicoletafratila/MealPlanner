using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse>
    {
        public RecipeEditModel? Model { get; set; }
    }
}
