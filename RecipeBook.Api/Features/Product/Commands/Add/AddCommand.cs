using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.Add
{
    public class AddCommand : IRequest<AddCommandResponse>
    {
        public EditProductModel? Model { get; set; }
    }
}
