using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.SearchByCategories
{
    public class SearchByCategoriesQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<SearchByCategoriesQuery, IList<ProductCategoryModel>>
    {
        public async Task<IList<ProductCategoryModel>> Handle(SearchByCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categoryIds =
                string.IsNullOrWhiteSpace(request.CategoryIds)
                ? Array.Empty<int>()
                : request.CategoryIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.Parse(id.Trim()))
                    .ToArray();

            var data = await repository.GetAllAsync();
            if (data != null && request.CategoryIds != null && request.CategoryIds.Length != 0)
            {
                data = data.Where(item => categoryIds.Contains(item.Id)).ToList();
            }

            return mapper.Map<IList<ProductCategoryModel>>(data);
        }
    }
}
