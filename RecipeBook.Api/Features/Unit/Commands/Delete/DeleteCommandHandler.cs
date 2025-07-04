using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    public class DeleteCommandHandler(IUnitRepository unitRepository, IRecipeIngredientRepository recipeIngredientRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IUnitRepository _unitRepository = unitRepository;
        private readonly IRecipeIngredientRepository _recipeIngredientRepository = recipeIngredientRepository;
        private readonly ILogger<DeleteCommandHandler> _logger = logger;

        public async Task<DeleteCommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _unitRepository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteCommandResponse { Message = $"Could not find with id {request.Id}." };
                }

                var products = await _recipeIngredientRepository.GetAllAsync();
                if (products!.Any(item => item.UnitId == request.Id))
                {
                    return new DeleteCommandResponse { Message = $"Unit {itemToDelete.Name} can not be deleted, it is used in products." };
                }

                await _unitRepository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occurred when deleting the unit." };
            }
        }
    }
}
