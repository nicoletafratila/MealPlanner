using MediatR;
using RecipeBook.Api.Repositories;

namespace MealPlanner.Api.Features.ProductCategory.Commands.DeleteProductCategory
{
    public class DeleteProductCategoryCommandHandler(IProductCategoryRepository repository, IProductRepository productRepository, ILogger<DeleteProductCategoryCommandHandler> logger) : IRequestHandler<DeleteProductCategoryCommand, DeleteProductCategoryCommandResponse>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ILogger<DeleteProductCategoryCommandHandler> _logger = logger;

        public async Task<DeleteProductCategoryCommandResponse> Handle(DeleteProductCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await _repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return new DeleteProductCategoryCommandResponse { Message = $"Could not find with id {request.Id}." };
                }

                var products = await _productRepository.GetAllAsync();
                if (products!.Any(item => item.ProductCategoryId == request.Id))
                {
                    return new DeleteProductCategoryCommandResponse { Message = $"Product category {request.Id} can not be deleted, it is used in products." };
                }

                await _repository.DeleteAsync(itemToDelete!);
                return new DeleteProductCategoryCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new DeleteProductCategoryCommandResponse { Message = "An error occured when deleting the product category." };
            }
        }
    }
}
