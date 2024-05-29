using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public UnitEditModel? Model { get; set; }
    }
}
