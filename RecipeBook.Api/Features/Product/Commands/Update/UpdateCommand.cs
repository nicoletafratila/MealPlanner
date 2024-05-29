using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public ProductEditModel? Model { get; set; }
    }
}
