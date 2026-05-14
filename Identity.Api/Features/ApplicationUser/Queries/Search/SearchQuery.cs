using Common.Pagination;
using Identity.Shared.Models;
using MediatR;

namespace Identity.Api.Features.ApplicationUser.Queries.Search
{
    public class SearchQuery : IRequest<PagedList<ApplicationUserModel>>
    {
        public QueryParameters<ApplicationUserModel>? QueryParameters { get; set; }
    }
}
