using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeCategoryRepository repository, IRecipeRepository RecipeRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, DeleteCommandResponse>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IRecipeRepository _RecipeRepository = RecipeRepository;
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

                var Recipes = await _RecipeRepository.GetAllAsync();
                if (Recipes!.Any(item => item.RecipeCategoryId == request.Id))
                {
                    return new DeleteCommandResponse { Message = $"Recipe category {request.Id} can not be deleted, it is used in Recipes." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteCommandResponse { Message = "An error occured when deleting the Recipe category." };
            }
        }
    }
}
