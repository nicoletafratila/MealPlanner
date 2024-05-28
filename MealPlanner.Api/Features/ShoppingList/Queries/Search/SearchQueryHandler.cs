using AutoMapper;
using Common.Pagination;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    public class SearchQueryHandler(IShoppingListRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ShoppingListModel>>
    {
        private readonly IShoppingListRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<ShoppingListModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<ShoppingListModel>>(data).OrderBy(r => r.Name).ToList();
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
