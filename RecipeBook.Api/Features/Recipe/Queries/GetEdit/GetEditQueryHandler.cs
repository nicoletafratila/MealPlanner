using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    public class GetEditQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, EditRecipeModel>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditRecipeModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeIngredientsAsync(request.Id);
            return _mapper.Map<EditRecipeModel>(result);
        }
    }
}
