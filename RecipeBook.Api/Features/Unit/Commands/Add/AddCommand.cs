using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse?>
    {
        public UnitEditModel? Model { get; set; }
    }
}
