using MediatR;

namespace RecipeBook.Api.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<DeleteProductCommandResponse>
    {
        public int Id { get; set; }
    }
}
