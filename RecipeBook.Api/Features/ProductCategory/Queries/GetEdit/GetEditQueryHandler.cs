using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.ProductCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, EditProductCategoryModel>
    {
        private readonly IProductCategoryRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditProductCategoryModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditProductCategoryModel>(result);
        }
    }
}
