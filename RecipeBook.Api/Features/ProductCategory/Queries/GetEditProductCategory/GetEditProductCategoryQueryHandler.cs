using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Queries.GetEditProductCategory
{
    public class GetEditProductCategoryQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditProductCategoryQuery, EditProductCategoryModel>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditProductCategoryModel> Handle(GetEditProductCategoryQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditProductCategoryModel>(result);
        }
    }
}
