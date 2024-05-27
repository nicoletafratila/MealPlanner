using AutoMapper;
using MealPlanner.Api.Features.ProductCategory.Commands.ProductCategory;
using MediatR;
using RecipeBook.Api.Repositories;

namespace MealPlanner.Api.Features.ProductCategory.Commands.AddProductCategory
{
    public class AddProductCategoryCommandHandler(IProductCategoryRepository repository, IMapper mapper, ILogger<AddProductCategoryCommandHandler> logger) : IRequestHandler<AddProductCategoryCommand, AddProductCategoryCommandResponse>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddProductCategoryCommandHandler> _logger = logger;

        public async Task<AddProductCategoryCommandResponse> Handle(AddProductCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ProductCategorys = await _repository.GetAllAsync();
                var existingItem = ProductCategorys?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddProductCategoryCommandResponse { Id = 0, Message = "This product category already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.ProductCategory>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddProductCategoryCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddProductCategoryCommandResponse { Message = "An error occured when saving the product category." };
            }
        }
    }
}
