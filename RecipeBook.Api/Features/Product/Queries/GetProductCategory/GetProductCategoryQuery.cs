using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetProductCategory
{
    public class GetProductCategoryQuery : IRequest<IList<ProductCategoryModel>>
    {
    }
}
