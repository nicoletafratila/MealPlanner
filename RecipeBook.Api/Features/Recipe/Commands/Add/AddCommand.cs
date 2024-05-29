using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public RecipeEditModel? Model { get; set; }
    }
}
