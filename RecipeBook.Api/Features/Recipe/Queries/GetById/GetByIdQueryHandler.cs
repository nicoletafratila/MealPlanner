using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    public class GetByIdQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<GetByIdQuery, RecipeModel>
    {
        public async Task<RecipeModel> Handle(GetByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<RecipeModel>(result);
        }
    }
}
