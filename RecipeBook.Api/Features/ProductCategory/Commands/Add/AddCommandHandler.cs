using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommandHandler(IProductCategoryRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, CommandResponse?>
    {
        public async Task<CommandResponse?> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ProductCategorys = await repository.GetAllAsync();
                var existingItem = ProductCategorys?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return CommandResponse.Failed("This product category already exists.");

                var mapped = mapper.Map<Common.Data.Entities.ProductCategory>(request.Model);
                var newItem = await repository.AddAsync(mapped);
                return CommandResponse.Success();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return CommandResponse.Failed("An error occurred when saving the product category.");
            }
        }
    }
}
