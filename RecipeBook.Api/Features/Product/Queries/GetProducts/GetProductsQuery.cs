using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<IList<ProductModel>>
    {
    }
}
