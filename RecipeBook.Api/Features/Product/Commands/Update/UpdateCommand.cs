using Common.Models;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Update
{
    public class UpdateCommand : IRequest<CommandResponse>
    {
        public ProductEditModel? Model { get; set; }
    }
}
