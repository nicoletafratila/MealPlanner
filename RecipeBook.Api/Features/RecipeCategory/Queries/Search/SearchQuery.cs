using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<RecipeCategoryModel>>
    {
        public QueryParameters<RecipeCategoryModel>? QueryParameters { get; set; }
    }
}
