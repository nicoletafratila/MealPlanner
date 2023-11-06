using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetProductCategories
{
    public class GetProductCategoriesQuery : IRequest<IList<ProductCategoryModel>>
    {
    }
}
