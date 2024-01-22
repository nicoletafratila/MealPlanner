using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.UpdateRecipe
{
    public class UpdateRecipeCommandHandler(IRecipeRepository repository, IMapper mapper, ILogger<UpdateRecipeCommandHandler> logger) : IRequestHandler<UpdateRecipeCommand, UpdateRecipeCommandResponse>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateRecipeCommandHandler> _logger = logger;

        public async Task<UpdateRecipeCommandResponse> Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdIncludeIngredientsAsync(request.Model!.Id);
                if (existingItem == null)
                    return new UpdateRecipeCommandResponse { Message = $"Could not find with id {request.Model!.Id}" };

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateRecipeCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateRecipeCommandResponse { Message = "An error occured when saving the recipe." };
            }
        }
    }
}
