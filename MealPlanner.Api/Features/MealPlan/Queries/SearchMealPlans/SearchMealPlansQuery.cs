using Common.Pagination;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlans
{
    public class SearchMealPlansQuery : IRequest<PagedList<MealPlanModel>>
    {
        public QueryParameters? QueryParameters { get; set; }
    }
}
