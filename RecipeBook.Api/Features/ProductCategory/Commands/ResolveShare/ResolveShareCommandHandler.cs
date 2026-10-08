using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Api.Services;

namespace RecipeBook.Api.Features.ProductCategory.Commands.ResolveShare
{
    /// <summary>
    /// Handles resolving (or creating, if missing) the target user's equivalents of a set of product categories.
    /// </summary>
    public class ResolveShareCommandHandler(
        IProductCategoryRepository repository,
        IProductCategoryShareResolver resolver) : IRequestHandler<ResolveShareCommand, Dictionary<Guid, Guid>>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IProductCategoryShareResolver _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        public async Task<Dictionary<Guid, Guid>> Handle(ResolveShareCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var allCategories = await _repository.GetAllAsync(cancellationToken) ?? [];
            var sourceCategories = allCategories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToList();

            return await _resolver.ResolveAsync(sourceCategories, request.TargetUserId, cancellationToken);
        }
    }
}
