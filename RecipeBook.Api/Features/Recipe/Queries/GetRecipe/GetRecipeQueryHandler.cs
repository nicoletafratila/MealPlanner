using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipe
{
    public class GetRecipeQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetRecipeQuery, RecipeModel>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<RecipeModel> Handle(GetRecipeQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<RecipeModel>(result);
        }
    }
}
