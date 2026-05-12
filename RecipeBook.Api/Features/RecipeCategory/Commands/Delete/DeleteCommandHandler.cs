using Common.Models;
using MediatR;
using RecipeBook.Api.Features.RecipeCategory.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    /// <summary>
    /// Handles deletion of recipe categories. Prevents deletion when the category is used by recipes.
    /// </summary>
    public class DeleteCommandHandler(
        IRecipeCategoryRepository repository,
        IRecipeRepository recipeRepository,
        ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        private readonly IRecipeCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly ILogger<DeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id, cancellationToken);
                if (itemToDelete is null)
                {
                    return CommandResponse.Failed(string.Format(RecipeCategoryMessages.NotFoundById, request.Id));
                }

                var recipes = await _recipeRepository.GetAllAsync(cancellationToken) ?? [];

                if (recipes.Any(r => r.RecipeCategoryId == request.Id))
                {
                    return CommandResponse.Failed(
                        string.Format(RecipeCategoryMessages.RecipeCategoryUsedInRecipes, itemToDelete.Name));
                }

                await _repository.DeleteAsync(itemToDelete, cancellationToken);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when deleting the recipe category with id {Id}.", request.Id);
                return CommandResponse.Failed(RecipeCategoryMessages.DeleteFailed);
            }
        }
    }
}