using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.DeleteShoppingList
{
    public class DeleteShoppingListCommandHandler : IRequestHandler<DeleteShoppingListCommand, DeleteShoppingListCommandResponse>
    {
        private readonly IShoppingListRepository _repository;
        private readonly ILogger<DeleteShoppingListCommandHandler> _logger;

        public DeleteShoppingListCommandHandler(IShoppingListRepository repository, ILogger<DeleteShoppingListCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DeleteShoppingListCommandResponse> Handle(DeleteShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteShoppingListCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteShoppingListCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteShoppingListCommandResponse { Message = "An error occured when deleting the shopping list." };
            }
        }
    }
}
