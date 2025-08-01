using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    public class AddCommand : IRequest<CommandResponse>
    {
        public MealPlanEditModel? Model { get; set; }
    }
}
