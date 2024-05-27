using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace MealPlanner.Api.Features.ProductCategory.Commands.UpdateProductCategory
{
    public class UpdateProductCategoryCommandHandler(IProductCategoryRepository repository, IMapper mapper, ILogger<UpdateProductCategoryCommandHandler> logger) : IRequestHandler<UpdateProductCategoryCommand, UpdateProductCategoryCommandResponse>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateProductCategoryCommandHandler> _logger = logger;

        public async Task<UpdateProductCategoryCommandResponse> Handle(UpdateProductCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdAsync(request.Model!.Id);
                if (existingItem == null)
                    return new UpdateProductCategoryCommandResponse { Message = $"Could not find with id {request.Model?.Id}" };
                 
                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateProductCategoryCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateProductCategoryCommandResponse { Message = "An error occured when saving the product category." };
            }
        }
    }
}
