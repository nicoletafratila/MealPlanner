using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQueryHandler(
        IProductCategoryRepository repository,
        IMapper mapper)
                : IRequestHandler<SearchByCategoriesQuery, IList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<IList<ProductCategoryModel>> Handle(
            SearchByCategoriesQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var categoryIds = ParseCategoryIds(request.CategoryIds);

            var data = await _repository.GetAllAsync() ?? [];

            if (categoryIds.Count > 0)
            {
                data = data.Where(item => categoryIds.Contains(item.Id)).ToList();
            }

            return _mapper.Map<IList<ProductCategoryModel>>(data);
        }

        private static List<int> ParseCategoryIds(string? categoryIds)
        {
            if (string.IsNullOrWhiteSpace(categoryIds))
                return [];

            var result = new List<int>();

            foreach (var token in categoryIds
                         .Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => s.Trim()))
            {
                if (int.TryParse(token, out var id))
                {
                    result.Add(id);
                }
            }

            return result;
        }
    }
}