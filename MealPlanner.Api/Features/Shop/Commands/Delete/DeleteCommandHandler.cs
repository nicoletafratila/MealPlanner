using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Delete
{
    public class DeleteCommandHandler(IShopRepository repository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IShopRepository _repository = repository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;

        public async Task<DeleteCommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occurred when deleting the shop." };
            }
        }
    }
}
