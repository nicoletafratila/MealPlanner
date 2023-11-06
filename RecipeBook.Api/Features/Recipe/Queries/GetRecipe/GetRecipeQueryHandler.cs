using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetRecipe
{
    public class GetRecipeQueryHandler : IRequestHandler<GetRecipeQuery, RecipeModel>
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;

        public GetRecipeQueryHandler(IRecipeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<RecipeModel> Handle(GetRecipeQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<RecipeModel>(result);
        }
    }
}
