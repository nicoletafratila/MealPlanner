using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.Search
{
    public class SearchQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ShopModel>>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<ShopModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<ShopModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<ShopModel>()).ToList();
                    }
                }

                if (!string.IsNullOrEmpty(request.QueryParameters!.SortString))
                {
                    results = results.AsQueryable().OrderByPropertyName(request.QueryParameters.SortString, request.QueryParameters.SortDirection).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<ShopModel>(new List<ShopModel>(), new Metadata());
        }
    }
}
