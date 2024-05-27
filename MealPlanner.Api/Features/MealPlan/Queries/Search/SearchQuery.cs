using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.Search
{ 
    public class SearchQuery : IRequest<PagedList<MealPlanModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
