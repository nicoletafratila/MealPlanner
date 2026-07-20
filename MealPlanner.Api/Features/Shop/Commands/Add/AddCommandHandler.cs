using AutoMapper;
using Common.Models;
using Common.Services;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Add
{
    /// <summary>
    /// Handles adding a new shop.
    /// </summary>
    public class AddCommandHandler(
        IShopRepository repository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<AddCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Model is null)
                throw new ArgumentNullException(nameof(request), Resources.ShopMessages.ModelCannotBeNull);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(Resources.ShopMessages.UserIdRequired);

                var shops = await _repository.GetAllByUserAsync(userId, cancellationToken);
                var name = request.Model.Name ?? string.Empty;

                var existingItem = shops?
                    .FirstOrDefault(i =>
                        !string.IsNullOrWhiteSpace(i.Name) &&
                        i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (existingItem is not null)
                    return CommandResponse.Failed(Resources.ShopMessages.AlreadyExists);

                var mapped = _mapper.Map<Data.Entities.Shop>(request.Model);
                mapped.Id = Guid.NewGuid();
                mapped.UserId = userId;
                await _repository.AddAsync(mapped, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when saving the shop '{Name}'.", request.Model.Name);
                return CommandResponse.Failed(Resources.ShopMessages.SaveError);
            }
        }
    }
}