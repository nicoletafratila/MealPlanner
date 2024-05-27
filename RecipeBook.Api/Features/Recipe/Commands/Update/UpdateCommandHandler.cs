using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.Update
{
    public class UpdateCommandHandler(IRecipeRepository repository, IMapper mapper, ILogger<UpdateCommandHandler> logger) : IRequestHandler<UpdateCommand, UpdateCommandResponse>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UpdateCommandHandler> _logger = logger;

        public async Task<UpdateCommandResponse> Handle(UpdateCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingItem = await _repository.GetByIdIncludeIngredientsAsync(request.Model?.Id);
                if (existingItem == null)
                    return new UpdateCommandResponse { Message = $"Could not find with id {request.Model?.Id}" };

                _mapper.Map(request.Model, existingItem);
                await _repository.UpdateAsync(existingItem);
                return new UpdateCommandResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new UpdateCommandResponse { Message = "An error occured when saving the recipe." };
            }
        }
    }
}
