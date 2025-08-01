using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    public class GetEditQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ShopEditModel>
    {
        public async Task<ShopEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdIncludeDisplaySequenceAsync(request.Id);
            return mapper.Map<ShopEditModel>(result);
        }
    }
}
