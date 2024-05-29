using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public ProductEditModel? Model { get; set; }
    }
}
