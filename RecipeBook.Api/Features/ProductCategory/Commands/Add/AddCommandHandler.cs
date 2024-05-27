using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace MealPlanner.Api.Features.ProductCategory.Commands.Add
{
    public class AddCommandHandler(IProductCategoryRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, AddCommandResponse>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddCommandHandler> _logger = logger;

        public async Task<AddCommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var ProductCategorys = await _repository.GetAllAsync();
                var existingItem = ProductCategorys?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddCommandResponse { Id = 0, Message = "This product category already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.ProductCategory>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddCommandResponse { Message = "An error occured when saving the product category." };
            }
        }
    }
}
