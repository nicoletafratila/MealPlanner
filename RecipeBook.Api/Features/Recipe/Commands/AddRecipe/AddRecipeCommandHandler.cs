using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.AddRecipe
{
    public class AddRecipeCommandHandler : IRequestHandler<AddRecipeCommand, AddRecipeCommandResponse>
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<AddRecipeCommandHandler> _logger;

        public AddRecipeCommandHandler(IRecipeRepository repository, IMapper mapper, ILogger<AddRecipeCommandHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AddRecipeCommandResponse> Handle(AddRecipeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.SearchAsync(request.Model!.Name!);
                if (existingItem != null)
                    return new AddRecipeCommandResponse { Id = 0, Message = "This recipe already exists in this category." };

                var mapped = _mapper.Map<Common.Data.Entities.Recipe>(request.Model);
                var newItem = await _repository.AddAsync(mapped);
                return new AddRecipeCommandResponse { Id = newItem.Id, Message = string.Empty };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new AddRecipeCommandResponse { Message = "An error occured when saving the recipe." };
            }
        }
    }
}
