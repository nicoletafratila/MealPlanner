using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Delete
{
    public class DeleteCommandHandler(IShoppingListRepository repository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}");
                }

                await repository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the shopping list.");
            }
        }
    }
}
