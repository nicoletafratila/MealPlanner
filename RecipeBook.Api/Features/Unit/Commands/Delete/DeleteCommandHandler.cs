using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    /// <summary>
    /// Handles deletion of units. Prevents deletion when the unit is used in recipe ingredients.
    /// </summary>
    public class DeleteCommandHandler(
        IUnitRepository unitRepository,
        IRecipeIngredientRepository recipeIngredientRepository,
        ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        private readonly IUnitRepository _unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
        private readonly IRecipeIngredientRepository _recipeIngredientRepository = recipeIngredientRepository ?? throw new ArgumentNullException(nameof(recipeIngredientRepository));
        private readonly ILogger<DeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var itemToDelete = await _unitRepository.GetByIdAsync(request.Id);
                if (itemToDelete is null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}.");
                }

                var ingredients = await _recipeIngredientRepository.GetAllAsync() ?? [];

                if (ingredients.Any(i => i.UnitId == request.Id))
                {
                    return CommandResponse.Failed(
                        $"Unit {itemToDelete.Name} can not be deleted, it is used in products.");
                }

                await _unitRepository.DeleteAsync(itemToDelete);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when deleting the unit with id {Id}.", request.Id);
                return CommandResponse.Failed("An error occurred when deleting the unit.");
            }
        }
    }
}