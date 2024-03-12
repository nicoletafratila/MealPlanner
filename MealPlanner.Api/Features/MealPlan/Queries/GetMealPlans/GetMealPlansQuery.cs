using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetMealPlans
{
    public class GetMealPlansQuery : IRequest<IList<MealPlanModel>>
    {
    }
}
