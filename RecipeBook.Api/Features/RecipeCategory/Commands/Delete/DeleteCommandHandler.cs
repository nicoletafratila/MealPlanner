using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    public class DeleteCommandHandler(IRecipeCategoryRepository repository, IRecipeRepository recipeRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}.");
                }

                var recipes = await recipeRepository.GetAllAsync();
                if (recipes!.Any(item => item.RecipeCategoryId == request.Id))
                {
                    return CommandResponse.Failed($"Recipe category {itemToDelete.Name} can not be deleted, it is used in recipes.");
                }

                await repository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the recipe category.");
            }
        }
    }
}
