using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe
{
    public class GetEditRecipeQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetEditRecipeQuery, EditRecipeModel>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditRecipeModel> Handle(GetEditRecipeQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeIngredientsAsync(request.Id);
            return _mapper.Map<EditRecipeModel>(result);
        }
    }
}
