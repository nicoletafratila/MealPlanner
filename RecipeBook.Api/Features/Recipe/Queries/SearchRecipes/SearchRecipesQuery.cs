using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.SearchRecipes
{
    public class SearchRecipesQuery : IRequest<PagedList<RecipeModel>>
    {
        public string? CategoryId { get; set; }
        public QueryParameters? QueryParameters { get; set; }
    }
}
