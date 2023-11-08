using MediatR;

namespace RecipeBook.Api.Features.Product.Commands.AddProduct
{
    public class AddProductCommand : IRequest<AddProductCommandResponse>
    {
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public int UnitId { get; set; }
        public int ProductCategoryId { get; set; }
    }
}
