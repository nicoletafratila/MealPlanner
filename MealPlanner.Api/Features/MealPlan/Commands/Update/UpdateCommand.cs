using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Update
{
    public class UpdateCommand : IRequest<UpdateCommandResponse>
    {
        public MealPlanEditModel? Model { get; set; }
    }
}
