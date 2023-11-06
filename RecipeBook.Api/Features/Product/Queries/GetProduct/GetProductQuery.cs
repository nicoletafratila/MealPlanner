using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetProduct
{
    public class GetProductQuery : IRequest<IList<ProductModel>>
    {
    }
}
