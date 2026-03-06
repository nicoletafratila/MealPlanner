using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Delete
{
    /// <summary>
    /// Handles deletion of product categories. Prevents deletion when used by products.
    /// </summary>
    public class DeleteCommandHandler(
        IProductCategoryRepository repository,
        IProductRepository productRepository,
        ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse?>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        private readonly ILogger<DeleteCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<CommandResponse?> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete is null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}.");
                }

                var products = await _productRepository.GetAllAsync() ?? [];

                if (products.Any(item => item.ProductCategoryId == request.Id))
                {
                    return CommandResponse.Failed($"Product category '{itemToDelete.Name}' can not be deleted, it is used in products.");
                }

                await _repository.DeleteAsync(itemToDelete);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred when deleting the product category with id {Id}.", request.Id);
                return CommandResponse.Failed("An error occurred when deleting the product category.");
            }
        }
    }
}