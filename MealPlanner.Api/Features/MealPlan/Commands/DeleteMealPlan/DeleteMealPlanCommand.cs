using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.DeleteMealPlan
{
    public class DeleteMealPlanCommand : IRequest<DeleteMealPlanCommandResponse>
    {
        public int Id { get; set; }
    }
}
