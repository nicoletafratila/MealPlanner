using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Product.Commands.Delete
{
    /// <summary>
    /// Handles deletion of products. Prevents deletion when used in recipes.
    /// </summary>
    public class DeleteCommandHandler(
        IProductRepository repository,
        IRecipeIngredientRepository recipeIngredientRepository,
        ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IRecipeIngredientRepository _recipeIngredientRepository = recipeIngredientRepository ?? throw new ArgumentNullException(nameof(recipeIngredientRepository));
        private readonly ILogger<DeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete is null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}");
                }

                var ingredients = await _recipeIngredientRepository.SearchAsync(request.Id) ?? [];

                if (ingredients.Any())
                {
                    return CommandResponse.Failed(
                        $"Product '{itemToDelete.Name}' can not be deleted, it is used in recipes.");
                }

                await _repository.DeleteAsync(itemToDelete);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when deleting the product with id {Id}.", request.Id);
                return CommandResponse.Failed("An error occurred when deleting the product.");
            }
        }
    }
}