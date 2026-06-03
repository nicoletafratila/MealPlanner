using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Commands.Delete
{
    /// <summary>
    /// Command to delete a meal plan by id.
    /// </summary>
    public class DeleteCommand : IRequest<CommandResponse?>
    {
        /// <summary>
        /// Id of the meal plan to delete.
        /// </summary>
        public Guid Id { get; set; }

        public DeleteCommand()
        {
        }

        public DeleteCommand(Guid id)
        {
            Id = id;
        }
    }
}