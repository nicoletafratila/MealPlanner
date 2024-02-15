using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.SearchRecipes
{
    public class SearchRecipesQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<SearchRecipesQuery, PagedList<RecipeModel>>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<RecipeModel>> Handle(SearchRecipesQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(request.CategoryId))
            {
                data = await _repository.SearchAsync(int.Parse(request.CategoryId));
            }
            var results = _mapper.Map<IList<RecipeModel>>(data).OrderBy(item => item.RecipeCategory?.DisplaySequence).ThenBy(item => item.Name);
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
