using MediatR;

namespace RecipeBook.Api.Features.Product.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest<UpdateProductCommandResponse>
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public byte[]? ImageContent { get; set; }
        public int UnitId { get; set; }
        public int ProductCategoryId { get; set; }
    }
}
