using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeCategoryRepository repository, IRecipeRepository recipeRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IRecipeRepository _recipeRepository = recipeRepository;
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

                var recipes = await _recipeRepository.GetAllAsync();
                if (recipes!.Any(item => item.RecipeCategoryId == request.Id))
                {
                    return new DeleteCommandResponse { Message = $"Recipe category {itemToDelete.Name} can not be deleted, it is used in recipes." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occurred when deleting the recipe category." };
            }
        }
    }
}
