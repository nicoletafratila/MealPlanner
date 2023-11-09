using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Commands.AddRecipe
{
    public class AddRecipeCommandHandler : IRequestHandler<AddRecipeCommand, AddRecipeCommandResponse>
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;

        public AddRecipeCommandHandler(IRecipeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<AddRecipeCommandResponse> Handle(AddRecipeCommand request, CancellationToken cancellationToken)
        {
            var mapped = _mapper.Map<Common.Data.Entities.Recipe>(request.Model);
            var newItem = await _repository.AddAsync(mapped);
            return new AddRecipeCommandResponse { Id = newItem.Id, Message = string.Empty };
        }
    }
}
