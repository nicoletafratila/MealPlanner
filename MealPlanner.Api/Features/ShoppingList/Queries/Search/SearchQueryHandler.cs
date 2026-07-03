using AutoMapper;
using Common.Pagination;
using Common.Services;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.Search
{
    public class SearchQueryHandler(IShoppingListRepository repository, IMapper mapper, ICurrentUserService currentUserService) : IRequestHandler<SearchQuery, PagedList<ShoppingListModel>>
    {
        private readonly IShoppingListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        public async Task<PagedList<ShoppingListModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId))
                return new([], new Metadata());

            var entities = await _repository.GetAllByUserAsync(userId, cancellationToken);
            var models = _mapper.Map<IList<ShoppingListModel>>(entities) ?? [];

            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<ShoppingListModel> ApplyFilters(
            IList<ShoppingListModel> source,
            QueryParameters<ShoppingListModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<ShoppingListModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<ShoppingListModel> ApplySorting(
            IList<ShoppingListModel> source,
            QueryParameters<ShoppingListModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            return source.AsQueryable()
                         .ApplySorting(parameters.Sorting)!
                         .ToList();
        }
    }
}