using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEditProduct
{
    public class GetEditProductQuery : IRequest<EditProductModel>
    {
        public int Id { get; set; }
    }
}
