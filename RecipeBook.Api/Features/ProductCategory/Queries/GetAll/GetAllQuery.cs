using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetAll
{
    public class GetAllQuery : IRequest<IList<ProductCategoryModel>>
    {
    }
}
