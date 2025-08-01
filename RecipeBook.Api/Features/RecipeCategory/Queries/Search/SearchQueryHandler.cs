using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.Search
{
    public class SearchQueryHandler(IRecipeCategoryRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<RecipeCategoryModel>>
    {
        public async Task<PagedList<RecipeCategoryModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<RecipeCategoryModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<RecipeCategoryModel>()).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(request.QueryParameters!.SortString))
                {
                    results = results.AsQueryable().OrderByPropertyName(request.QueryParameters.SortString, request.QueryParameters.SortDirection).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<RecipeCategoryModel>(new List<RecipeCategoryModel>(), new Metadata());
        }
    }
}
