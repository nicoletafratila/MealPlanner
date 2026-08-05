using Common.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateProductCollected
{
    /// <summary>
    /// Command to toggle the collected state of a single shopping list product.
    /// </summary>
    public class UpdateProductCollectedCommand : IRequest<CommandResponse?>
    {
        public Guid ShoppingListId { get; set; }

        public Guid ProductId { get; set; }

        public bool Collected { get; set; }
    }
}
