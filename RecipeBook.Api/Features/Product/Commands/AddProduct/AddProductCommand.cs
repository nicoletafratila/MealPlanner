using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommand : IRequest<AddProductCommandResponse>
    {
        public EditProductModel? Model { get; set; }
    }
}
