using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetProductCategories
{
    public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery, IList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetProductCategoriesQueryHandler(IProductCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<ProductCategoryModel>> Handle(GetProductCategoriesQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<ProductCategoryModel>>(results).OrderBy(r => r.DisplaySequence).ToList();
        }
    }
}
