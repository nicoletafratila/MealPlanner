using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Add
{
    /// <summary>
    /// Command to add a new meal plan.
    /// </summary>
    public class AddCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// The meal plan edit model to add. Must be non-null for a valid command.
        /// </summary>
        public MealPlanEditModel? Model { get; set; }

        public AddCommand()
        {
        }

        public AddCommand(MealPlanEditModel model)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
        }
    }
}