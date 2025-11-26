using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Unit.Commands.Delete
{
    public class DeleteCommandHandler(IUnitRepository unitRepository, IRecipeIngredientRepository recipeIngredientRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await unitRepository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}.");
                }

                var products = await recipeIngredientRepository.GetAllAsync();
                if (products!.Any(item => item.UnitId == request.Id))
                {
                    return CommandResponse.Failed($"Unit {itemToDelete.Name} can not be deleted, it is used in products.");
                }

                await unitRepository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the unit.");
            }
        }
    }
}
