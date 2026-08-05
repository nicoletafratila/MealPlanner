using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.UpdateProductCollected
{
    /// <summary>
    /// Handles toggling the collected state of a single shopping list product without touching the rest of the list.
    /// </summary>
    public class UpdateProductCollectedCommandHandler(
        IShoppingListRepository repository,
        ILogger<UpdateProductCollectedCommandHandler> logger) : IRequestHandler<UpdateProductCollectedCommand, CommandResponse?>
    {
        private readonly IShoppingListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILogger<UpdateProductCollectedCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UpdateProductCollectedCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var updated = await _repository.UpdateProductCollectedAsync(
                    request.ShoppingListId, request.ProductId, request.Collected, cancellationToken);

                return updated
                    ? CommandResponse.Success()
                    : CommandResponse.Failed(string.Format(Resources.ShoppingListMessages.NotFound, request.ShoppingListId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when updating the collected state for product {ProductId} on shopping list {ShoppingListId}.",
                    request.ProductId, request.ShoppingListId);

                return CommandResponse.Failed(Resources.ShoppingListMessages.SaveError);
            }
        }
    }
}
