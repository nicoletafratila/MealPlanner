using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.DeleteShop
{
    public class DeleteShopCommandHandler(IShopRepository repository, ILogger<DeleteShopCommandHandler> logger) : IRequestHandler<DeleteShopCommand, DeleteShopCommandResponse>
    {
        private readonly IShopRepository _repository = repository;
        private readonly ILogger<DeleteShopCommandHandler> _logger = logger;

        public async Task<DeleteShopCommandResponse> Handle(DeleteShopCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteShopCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteShopCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteShopCommandResponse { Message = "An error occured when deleting the shop." };
            }
        }
    }
}
