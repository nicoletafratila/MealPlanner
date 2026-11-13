using Common.Pagination;
using MediatR;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Job.Queries.Search
{
    /// <summary>
    /// Admin query to search background jobs across all users, with pagination and filtering.
    /// </summary>
    public class SearchQuery : IRequest<PagedList<JobModel>>
    {
        /// <summary>
        /// Pagination, filtering, and sorting parameters for jobs.
        /// </summary>
        public QueryParameters<JobModel>? QueryParameters { get; set; }
    }
}
