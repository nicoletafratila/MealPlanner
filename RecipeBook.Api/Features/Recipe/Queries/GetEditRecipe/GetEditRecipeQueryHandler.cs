using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEditRecipe
{
    public class GetEditRecipeQueryHandler : IRequestHandler<GetEditRecipeQuery, EditRecipeModel>
    {
        private readonly IRecipeRepository _repository;
        private readonly IMapper _mapper;

        public GetEditRecipeQueryHandler(IRecipeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EditRecipeModel> Handle(GetEditRecipeQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeIngredientsAsync(request.Id);
            return _mapper.Map<EditRecipeModel>(result);
        }
    }
}
