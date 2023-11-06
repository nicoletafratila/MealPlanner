using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipes
{
    public class GetRecipesQueryHandler : IRequestHandler<GetRecipesQuery, IList<RecipeModel>>
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;

        public GetRecipesQueryHandler(IRecipeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<RecipeModel>> Handle(GetRecipesQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<RecipeModel>>(results).OrderBy(item => item.RecipeCategory!.DisplaySequence).ThenBy(item => item.Name).ToList();
        }
    }
}
