using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.Delete
{
    public class DeleteCommandHandler(IProductRepository repository, IRecipeIngredientRepository recipeIngredientRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IProductRepository _repository = repository;
        private readonly IRecipeIngredientRepository _recipeIngredientRepository = recipeIngredientRepository;
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

                var result = await _recipeIngredientRepository.SearchAsync(request.Id);
                if (result != null && result.Any())
                {
                    return new DeleteCommandResponse { Message = $"Product '{itemToDelete.Name}' can not be deleted, it is used in recipes." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occured when deleting the product." };
            }
        }
    }
}
