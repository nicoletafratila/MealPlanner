using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetAll
{
    public class GetAllQuery : IRequest<IList<RecipeCategoryModel>>
    {
    }
}
