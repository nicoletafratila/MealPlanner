using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Recipe.Queries.Search
{
    public class SearchQueryHandler(IRecipeRepository repository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<RecipeModel>>
    {
        private readonly IRecipeRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<PagedList<RecipeModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            if (request?.QueryParameters is null)
            {
                return new([], new Metadata());
            }

            var qp = request.QueryParameters;

            var entities = await _repository.GetAllAsync();
            var models = _mapper.Map<IList<RecipeModel>>(entities) ?? [];

            models = ApplyCategoryFilter(models, request.CategoryId);
            models = ApplyFilters(models, qp);
            models = ApplySorting(models, qp);

            return models.ToPagedList(qp.PageNumber, qp.PageSize);
        }

        private static IList<RecipeModel> ApplyCategoryFilter(
            IList<RecipeModel> source,
            string? categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
                return source;

            if (!int.TryParse(categoryId, out var id))
                return source;

            return source.Where(r => r.RecipeCategoryId == id.ToString()).ToList();
        }

        private static IList<RecipeModel> ApplyFilters(
            IList<RecipeModel> source,
            QueryParameters<RecipeModel> parameters)
        {
            if (parameters.Filters is null || !parameters.Filters.Any())
                return source;

            var result = source;

            foreach (var filter in parameters.Filters)
            {
                var predicate = filter.ConvertFilterItemToFunc<RecipeModel>();
                result = result.Where(predicate).ToList();
            }

            return result;
        }

        private static IList<RecipeModel> ApplySorting(
            IList<RecipeModel> source,
            QueryParameters<RecipeModel> parameters)
        {
            if (parameters.Sorting is null || !parameters.Sorting.Any())
                return source;

            var sortingItems = parameters.Sorting
                .Select(QueryParameters<RecipeModel>.FromModel)
                .ToList();

            return source.AsQueryable()
                         .ApplySorting(sortingItems)!
                         .ToList();
        }
    }
}