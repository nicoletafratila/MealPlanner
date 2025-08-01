using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    public class GetEditQueryHandler(IProductCategoryRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductCategoryEditModel>
    {
        public async Task<ProductCategoryEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<ProductCategoryEditModel>(result);
        }
    }
}
