using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Delete
{
    public class DeleteCommandHandler(IProductCategoryRepository repository, IProductRepository productRepository, ILogger<DeleteCommandHandler> logger) : IRequestHandler<DeleteCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(DeleteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var itemToDelete = await repository.GetByIdAsync(request.Id);
                if (itemToDelete == null)
                {
                    return CommandResponse.Failed($"Could not find with id {request.Id}.");
                }

                var products = await productRepository.GetAllAsync();
                if (products!.Any(item => item.ProductCategoryId == request.Id))
                {
                    return CommandResponse.Failed($"Product category '{itemToDelete.Name}' can not be deleted, it is used in products.");
                }

                await repository.DeleteAsync(itemToDelete!);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when deleting the product category.");
            }
        }
    }
}
