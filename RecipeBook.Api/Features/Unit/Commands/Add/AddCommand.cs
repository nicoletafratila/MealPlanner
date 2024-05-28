using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditUnitModel? Model { get; set; }
    }
}
