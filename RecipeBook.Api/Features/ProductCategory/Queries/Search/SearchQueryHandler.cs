using AutoMapper;
using Common.Pagination;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQueryHandler(IProductCategoryRepository productCategoryRepository, IProductRepository productRepository, IMapper mapper) : IRequestHandler<SearchQuery, PagedList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _productCategoryRepository = productCategoryRepository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedList<ProductCategoryModel>> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var data = new List<Common.Data.Entities.ProductCategory>();
            var categories = await _productCategoryRepository.GetAllAsync();
            foreach (var category in categories!)
            {
                var products = await _productRepository.SearchAsync(category.Id);
                if (products != null && products.Any())
                {
                    data.Add(category);
                }
            }

            var results = _mapper.Map<IList<ProductCategoryModel>>(data).OrderBy(item => item.Name);
            return results.ToPagedList(request.QueryParameters!.PageNumber, request.QueryParameters.PageSize);
        }
    }
}
