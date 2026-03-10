using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Update
{
    /// <summary>
    /// Handles updating a shopping list (including its products).
    /// </summary>
    public class UpdateCommandHandler(
        IShoppingListRepository repository,
        IMapper mapper,
        ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, CommandResponse?>
    {
        private readonly IShoppingListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<UpdateCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), "Model cannot be null.");

            try
            {
                var existingItem = await _repository.GetByIdIncludeProductsAsync(request.Model.Id, cancellationToken);
                if (existingItem is null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Model.Id}");
                }

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when saving the shopping list with id {Id}.",
                    request.Model.Id);

                return CommandResponse.Failed("An error occurred when saving the meal plan.");
            }
        }
    }
}