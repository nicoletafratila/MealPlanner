using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteProductCommandResponse>
    {
        private readonly IProductRepository _repository;
        private readonly IRecipeIngredientRepository _recipeIngredientRepository;
        private readonly ILogger<DeleteProductCommandHandler> _logger;

        public DeleteProductCommandHandler(IProductRepository repository, IRecipeIngredientRepository recipeIngredientRepository, ILogger<DeleteProductCommandHandler> logger)
        {
            _repository = repository;
            _recipeIngredientRepository = recipeIngredientRepository;
            _logger = logger;
        }

        public async Task<DeleteProductCommandResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteProductCommandResponse { Message = $"Could not find with id {request.Id}" };
                }

                var result = await _recipeIngredientRepository.SearchAsync(request.Id);
                if (result != null && result.Any())
                {
                    return new DeleteProductCommandResponse { Message = "The product you try to delete is used in recipes and cannot be deleted." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteProductCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteProductCommandResponse { Message = "An error occured when deleting the product." };
            }
        }
    }
}
