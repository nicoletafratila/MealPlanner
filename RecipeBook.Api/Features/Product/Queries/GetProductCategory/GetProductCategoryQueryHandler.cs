using AutoMapper;
using MediatR;
using RecipeBook.Api.Features.Product.Queries.GetProductCategory;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetProducts
{
    public class GetProductCategoryQueryHandler : IRequestHandler<GetProductCategoryQuery, IList<ProductCategoryModel>>
    {
        private readonly IProductCategoryRepository _repository;
        private readonly IMapper _mapper;

        public GetProductCategoryQueryHandler(IProductCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<ProductCategoryModel>> Handle(GetProductCategoryQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<ProductCategoryModel>>(results).OrderBy(r => r.DisplaySequence).ToList();
        }
    }
}
