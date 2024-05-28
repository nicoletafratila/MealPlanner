using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    public class DeleteCommandHandler(IUnitRepository repository, IProductRepository productRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IUnitRepository _repository = repository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;

        public async Task<DeleteCommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteCommandResponse { Message = $"Could not find with id {request.Id}." };
                }

                var products = await _productRepository.GetAllAsync();
                if (products!.Any(item => item.UnitId == request.Id))
                {
                    return new DeleteCommandResponse { Message = $"Unit {itemToDelete.Name} can not be deleted, it is used in products." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occured when deleting the unit." };
            }
        }
    }
}
