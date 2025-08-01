using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    public class GetEditQueryHandler(IProductRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ProductEditModel>
    {
        public async Task<ProductEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<ProductEditModel>(result);
        }
    }
}
