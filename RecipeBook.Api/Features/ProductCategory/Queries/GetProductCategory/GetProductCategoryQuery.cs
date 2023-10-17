using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetProductCategory
{
    public class GetProductCategoryQuery : IRequest<IList<ProductCategoryModel>>
    {
    }
}
