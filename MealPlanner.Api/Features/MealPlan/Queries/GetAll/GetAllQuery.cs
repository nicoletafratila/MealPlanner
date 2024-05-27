using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetAll
{
    public class GetAllQuery : IRequest<IList<MealPlanModel>>
    {
    }
}
