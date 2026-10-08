using Common.Models;
using MealPlanner.Api.Abstractions;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Commands.Share
{
    /// <summary>
    /// Handles sharing a shop by cloning it, and its display sequence, into a new record owned by another user.
    /// </summary>
    public class ShareCommandHandler(
        IShopRepository repository,
        IRecipeBookClient recipeBookClient,
        ILogger<ShareCommandHandler> logger) : IRequestHandler<ShareCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IRecipeBookClient _recipeBookClient = recipeBookClient ?? throw new ArgumentNullException(nameof(recipeBookClient));
        private readonly ILogger<ShareCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var source = await _repository.GetByIdIncludeDisplaySequenceAsync(request.ShopId, cancellationToken);
                if (source is null)
                {
                    return CommandResponse.Failed(string.Format(Resources.ShopMessages.NotFound, request.ShopId));
                }

                var categoryIds = (source.DisplaySequence ?? [])
                    .Select(ds => ds.ProductCategoryId)
                    .Distinct()
                    .ToList();

                var categoryMap = categoryIds.Count > 0
                    ? await _recipeBookClient.ResolveShareProductCategoriesAsync(categoryIds, request.TargetUserId, request.AuthToken, cancellationToken) ?? []
                    : [];

                var uniqueName = await ResolveUniqueShopNameAsync(source.Name, request.TargetUserId, cancellationToken);

                var shared = new Data.Entities.Shop
                {
                    Name = uniqueName,
                    UserId = request.TargetUserId,
                    DisplaySequence = (source.DisplaySequence ?? [])
                        .Select(ds => new Data.Entities.ShopDisplaySequence
                        {
                            Value = ds.Value,
                            ProductCategoryId = categoryMap[ds.ProductCategoryId]
                        })
                        .ToList()
                };

                await _repository.AddAsync(shared, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing the shop with id {ShopId} to user {TargetUserId}.", request.ShopId, request.TargetUserId);
                return CommandResponse.Failed(Resources.ShopMessages.SaveError);
            }
        }

        private async Task<string?> ResolveUniqueShopNameAsync(string? name, string targetUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var existingNames = new HashSet<string>(
                (await _repository.GetAllByUserAsync(targetUserId, cancellationToken))
                    .Where(s => s.Name is not null)
                    .Select(s => s.Name!),
                StringComparer.OrdinalIgnoreCase);

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                if (!existingNames.Contains(candidate))
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            return candidate;
        }
    }
}
