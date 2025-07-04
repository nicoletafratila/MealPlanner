using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Add
{
    public class AddCommandHandler(IRecipeCategoryRepository repository, IMapper mapper, ILogger<AddCommandHandler> logger) : IRequestHandler<AddCommand, AddCommandResponse>
    {
        private readonly IRecipeCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddCommandHandler> _logger = logger;

        public async Task<AddCommandResponse> Handle(AddCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var RecipeCategorys = await _repository.GetAllAsync();
                var existingItem = RecipeCategorys?.FirstOrDefault(i => i.Name == request.Model?.Name!);
                if (existingItem != null)
                    return new AddCommandResponse { Id = 0, Message = "This Recipe category already exists." };

                var mapped = _mapper.Map<Common.Data.Entities.RecipeCategory>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddCommandResponse { Message = "An error occurred when saving the Recipe category." };
            }
        }
    }
}
