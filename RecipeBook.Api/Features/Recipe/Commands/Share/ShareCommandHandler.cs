using Common.Models;
using MediatR;
using RecipeBook.Api.Features.Recipe.Resources;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Share
{
    /// <summary>
    /// Handles sharing a recipe by cloning it into a new recipe owned by another user.
    /// </summary>
    public class ShareCommandHandler(
        IRecipeRepository repository,
        ILogger<ShareCommandHandler> logger) : IRequestHandler<ShareCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly ILogger<ShareCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var source = await _repository.GetByIdIncludeIngredientsAsync(request.RecipeId, cancellationToken);
                if (source is null)
                {
                    return CommandResponse.Failed(string.Format(RecipeMessages.NotFoundById, request.RecipeId));
                }

                var uniqueName = await ResolveUniqueNameAsync(source.Name, request.TargetUserId, cancellationToken);

                var shared = new Data.Entities.Recipe
                {
                    Name = uniqueName,
                    Source = source.Source,
                    ImageContent = source.ImageContent,
                    ImageThumbnail = source.ImageThumbnail,
                    RecipeCategoryId = source.RecipeCategoryId,
                    UserId = request.TargetUserId,
                    RecipeIngredients = source.RecipeIngredients?
                        .Select(ri => new Data.Entities.RecipeIngredient
                        {
                            ProductId = ri.ProductId,
                            UnitId = ri.UnitId,
                            Quantity = ri.Quantity
                        })
                        .ToList()
                };

                await _repository.AddAsync(shared, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing the recipe with id {RecipeId} to user {TargetUserId}.", request.RecipeId, request.TargetUserId);
                return CommandResponse.Failed(RecipeMessages.SaveFailed);
            }
        }

        private async Task<string?> ResolveUniqueNameAsync(string? name, string targetUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                var existing = await _repository.SearchAsync(candidate, targetUserId, cancellationToken);
                if (existing is null)
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            return candidate;
        }
    }
}
