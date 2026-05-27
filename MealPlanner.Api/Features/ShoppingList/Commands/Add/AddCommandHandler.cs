using AutoMapper;
using Common.Models;
using Common.Services;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.Add
{
    /// <summary>
    /// Handles adding a new shopping list.
    /// </summary>
    public class AddCommandHandler(
        IShoppingListRepository repository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IShoppingListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), Resources.ShoppingListMessages.ModelCannotBeNull);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(Resources.ShoppingListMessages.UserIdRequired);

                var name = request.Model.Name ?? string.Empty;

                var existingItem = await _repository.SearchAsync(name, userId, cancellationToken);
                if (existingItem is not null)
                    return CommandResponse.Failed(Resources.ShoppingListMessages.AlreadyExists);

                var mapped = _mapper.Map<MealPlanner.Data.Entities.ShoppingList>(request.Model);
                mapped.UserId = userId;
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred when saving the shopping list '{Name}'.",
                    request.Model.Name);
                return CommandResponse.Failed(Resources.ShoppingListMessages.SaveError);
            }
        }
    }
}