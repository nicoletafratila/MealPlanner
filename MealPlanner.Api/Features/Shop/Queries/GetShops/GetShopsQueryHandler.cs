using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetShops
{
    public class GetShopsQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetShopsQuery, IList<ShopModel>>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<ShopModel>> Handle(GetShopsQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<ShopModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
