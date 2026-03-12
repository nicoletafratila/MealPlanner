using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Update
{
    /// <summary>
    /// Command to update a meal plan.
    /// </summary>
    public class UpdateCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The meal plan edit model to update. Must be non-null for a valid command.
        /// </summary>
        public MealPlanEditModel? Model { get; set; }

        public UpdateCommand()
        {
        }

        public UpdateCommand(MealPlanEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}