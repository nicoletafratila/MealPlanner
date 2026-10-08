using Common.Models;
using Common.Services;
using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.ShareAll
{
    /// <summary>
    /// Handles sharing all of the current user's shops with another user by cloning them, and their display
    /// sequences, into new records.
    /// </summary>
    public class ShareAllCommandHandler(
        IShopRepository repository,
        IRecipeBookClient recipeBookClient,
        ICurrentUserService currentUserService,
        ILogger<ShareAllCommandHandler> logger) : IRequestHandler<ShareAllCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IRecipeBookClient _recipeBookClient = recipeBookClient ?? throw new ArgumentNullException(nameof(recipeBookClient));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<ShareAllCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareAllCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return CommandResponse.Failed(Resources.ShopMessages.UserIdRequired);

                var sourceShops = await _repository.GetAllByUserIncludeDisplaySequenceAsync(userId, request.Filters, cancellationToken);
                if (sourceShops.Count == 0)
                    return CommandResponse.Success();

                var categoryIds = sourceShops
                    .SelectMany(s => s.DisplaySequence ?? [])
                    .Select(ds => ds.ProductCategoryId)
                    .Distinct()
                    .ToList();

                var categoryMap = categoryIds.Count > 0
                    ? await _recipeBookClient.ResolveShareProductCategoriesAsync(categoryIds, request.TargetUserId, request.AuthToken, cancellationToken) ?? []
                    : [];

                var usedNames = new HashSet<string>(
                    (await _repository.GetAllByUserAsync(request.TargetUserId, cancellationToken))
                        .Where(s => s.Name is not null)
                        .Select(s => s.Name!),
                    StringComparer.OrdinalIgnoreCase);

                var sharedShops = sourceShops
                    .Select(source => new Data.Entities.Shop
                    {
                        Name = ResolveUniqueShopName(source.Name, usedNames),
                        UserId = request.TargetUserId,
                        DisplaySequence = (source.DisplaySequence ?? [])
                            .Select(ds => new Data.Entities.ShopDisplaySequence
                            {
                                Value = ds.Value,
                                ProductCategoryId = categoryMap[ds.ProductCategoryId]
                            })
                            .ToList()
                    })
                    .ToList();

                await _repository.AddRangeAsync(sharedShops, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing all shops to user {TargetUserId}.", request.TargetUserId);
                return CommandResponse.Failed(Resources.ShopMessages.SaveError);
            }
        }

        private static string? ResolveUniqueShopName(string? name, HashSet<string> usedNames)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                if (usedNames.Add(candidate))
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            usedNames.Add(candidate);
            return candidate;
        }
    }
}
