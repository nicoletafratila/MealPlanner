using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    public class GetEditQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, RecipeEditModel>
    {
        public async Task<RecipeEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdIncludeIngredientsAsync(request.Id);
            return mapper.Map<RecipeEditModel>(result);
        }
    }
}
