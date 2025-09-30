using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    public class SearchQueryHandler(IShoppingListRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ShoppingListModel>>
    {
        public async Task<PagedList<ShoppingListModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await repository.GetAllAsync();
            var results = mapper.Map<IList<ShoppingListModel>>(data);

            if (results != null && request.QueryParameters != null)
            {
                if (request.QueryParameters.Filters != null)
                {
                    foreach (var filter in request.QueryParameters.Filters)
                    {
                        results = results.Where(filter.ConvertFilterItemToFunc<ShoppingListModel>()).ToList();
                    }
                }

                if (request.QueryParameters!.Sorting != null && request.QueryParameters.Sorting.Any())
                {
                    var sortingItems = request.QueryParameters.Sorting.Select(QueryParameters<ShoppingListModel>.FromModel).ToList();
                    results = results.AsQueryable().ApplySorting(sortingItems).ToList();
                }

                return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
            }

            return new PagedList<ShoppingListModel>(new List<ShoppingListModel>(), new Metadata());
        }
    }
}
