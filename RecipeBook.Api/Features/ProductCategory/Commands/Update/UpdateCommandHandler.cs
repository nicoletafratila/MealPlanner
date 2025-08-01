using AutoMapper;
using Common.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.ProductCategory.Commands.Update
{
    public class UpdateCommandHandler(IProductCategoryRepository repository, IMapper mapper, ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, CommandResponse>
    {
        public async Task<CommandResponse> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await repository.GetByIdAsync(request.Model!.Id);
                if (existingItem == null)
                    return CommandResponse.Failed($"Could not find with id {request.Model?.Id}");
                 
                mapper.Map(request.Model, existingItem);
                await repository.UpdateAsync(existingItem);
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
