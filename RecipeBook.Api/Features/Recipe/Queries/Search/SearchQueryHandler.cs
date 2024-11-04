using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    public class SearchQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<RecipeModel>>
    {
        private readonly IRecipeRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<RecipeModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<RecipeModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<RecipeModel>()).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(request.QueryParameters!.SortString))
                {
                    results = results.AsQueryable().OrderByPropertyName(request.QueryParameters.SortString, request.QueryParameters.SortDirection).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<RecipeModel>(new List<RecipeModel>(), new Metadata());
        }
    }
}