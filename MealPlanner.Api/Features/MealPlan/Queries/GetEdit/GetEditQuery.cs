using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    public class GetEditMealPlanQuery : IRequest<MealPlanEditModel>
    {
        public int Id { get; set; }
    }
}
