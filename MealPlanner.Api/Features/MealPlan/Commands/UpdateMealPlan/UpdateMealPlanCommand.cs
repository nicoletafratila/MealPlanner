using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.UpdateMealPlan
{
    public class UpdateMealPlanCommand : IRequest<UpdateMealPlanCommandResponse>
    {
        public EditMealPlanModel? Model { get; set; }
    }
}
