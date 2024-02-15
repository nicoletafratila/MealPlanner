using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.SearchRecipeCategory
{
    public class SearchRecipeCategoryQuery : IRequest<PagedList<RecipeCategoryModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
