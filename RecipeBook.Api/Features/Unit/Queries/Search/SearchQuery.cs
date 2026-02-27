using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.Search
{
    /// <summary>
    /// Query for searching units with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<UnitModel>>
    {
        /// <summary>
        /// Parameters controlling filtering, sorting, and paging of units.
        /// </summary>
        public QueryParameters<UnitModel>? QueryParameters { get; set; }
    }
}