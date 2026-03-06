using Common.Models;
using MediatR;
using RecipeBook.Api.Abstractions;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    /// <summary>
    /// Handles deletion of recipes. Prevents deletion when used in meal plans.
    /// </summary>
    public class DeleteCommandHandler(
        IRecipeRepository repository,
        ILogger<DeleteCommandHandler> logger,
        IMealPlannerClient mealPlannerClient) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILogger<DeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMealPlannerClient _mealPlannerClient = mealPlannerClient ?? throw new ArgumentNullException(nameof(mealPlannerClient));

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

                var mealPlans = await _mealPlannerClient.GetMealPlansByRecipeIdAsync(request.Id, request.AuthToken, cancellationToken);
                if (mealPlans is not null && mealPlans.Count > 0)
                {
                    return CommandResponse.Failed($"Recipe {itemToDelete.Name} can not be deleted, it is used in meal plans.");
                }

                await _repository.DeleteAsync(itemToDelete);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when deleting the recipe with id {Id}.", request.Id);
                return CommandResponse.Failed("An error occurred when deleting the recipe.");
            }
        }
    }
}