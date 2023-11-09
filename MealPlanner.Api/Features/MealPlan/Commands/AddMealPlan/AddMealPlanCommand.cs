using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan
{
    public class AddMealPlanCommand : IRequest<AddMealPlanCommandResponse>
    {
        public EditMealPlanModel? Model { get; set; }
    }
}
