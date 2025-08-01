using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.Delete
{
    public class DeleteCommandHandler(IProductRepository repository, IRecipeIngredientRepository recipeIngredientRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}");
                }

                var result = await recipeIngredientRepository.SearchAsync(request.Id);
                if (result != null && result.Any())
                {
                    return CommandResponse.Failed($"Product '{itemToDelete.Name}' can not be deleted, it is used in recipes.");
                }

                await repository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the product.");
            }
        }
    }
}
