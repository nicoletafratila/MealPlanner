using Common.Models;
using MediatR;
using RecipeBook.Api.Features.Product.Resources;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Features.Product.Commands.Share
{
    /// <summary>
    /// Handles sharing a product by cloning it into a new record owned by another user.
    /// </summary>
    public class ShareCommandHandler(
        IProductRepository repository,
        IProductCategoryShareResolver productCategoryResolver,
        ILogger<ShareCommandHandler> logger) : IRequestHandler<ShareCommand, CommandResponse?>
    {
        private const int MaxNameAttempts = 50;

        private readonly IProductRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IProductCategoryShareResolver _productCategoryResolver = productCategoryResolver ?? throw new ArgumentNullException(nameof(productCategoryResolver));
        private readonly ILogger<ShareCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(ShareCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var source = await _repository.GetByIdAsync(request.ProductId, cancellationToken);
                if (source is null)
                {
                    return CommandResponse.Failed(string.Format(ProductMessages.NotFoundById, request.ProductId));
                }

                var categoryMap = await _productCategoryResolver.ResolveAsync(
                    [source.ProductCategory],
                    request.TargetUserId,
                    cancellationToken);

                var targetCategoryId = categoryMap[source.ProductCategoryId];
                var uniqueName = await ResolveUniqueProductNameAsync(source.Name, targetCategoryId, request.TargetUserId, cancellationToken);

                var shared = new Data.Entities.Product
                {
                    Name = uniqueName,
                    ImageContent = source.ImageContent,
                    ImageThumbnail = source.ImageThumbnail,
                    BaseUnitId = source.BaseUnitId,
                    ProductCategoryId = targetCategoryId,
                    UserId = request.TargetUserId
                };

                await _repository.AddAsync(shared, cancellationToken);

                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when sharing the product with id {ProductId} to user {TargetUserId}.", request.ProductId, request.TargetUserId);
                return CommandResponse.Failed(ProductMessages.SaveFailed);
            }
        }

        private async Task<string?> ResolveUniqueProductNameAsync(string? name, Guid categoryId, string targetUserId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            var candidate = name;
            for (var attempt = 1; attempt <= MaxNameAttempts; attempt++)
            {
                var existing = await _repository.SearchAsync(candidate, categoryId, targetUserId, cancellationToken);
                if (existing is null)
                    return candidate;

                candidate = attempt == 1 ? $"{name} (Copy)" : $"{name} (Copy {attempt})";
            }

            return candidate;
        }
    }
}
