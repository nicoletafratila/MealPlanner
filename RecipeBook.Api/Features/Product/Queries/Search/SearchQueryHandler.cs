using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductModel>>
    {
        public async Task<PagedList<ProductModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<ProductModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<ProductModel>()).ToList();
                    }
                }

                if (request.QueryParameters!.Sorting != null && request.QueryParameters.Sorting.Any())
                {
                    var sortingItems = request.QueryParameters.Sorting.Select(QueryParameters<ProductModel>.FromModel).ToList();
                    results = results.AsQueryable()!.ApplySorting(sortingItems)!.ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<ProductModel>(new List<ProductModel>(), new Metadata());
        }
    }
}
