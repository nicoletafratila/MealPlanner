using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetShop
{
    public class GetShopQueryHandler : IRequestHandler<GetShopQuery, ShopModel>
    {
        private readonly IShopRepository _repository;
        private readonly IMapper _mapper;

        public GetShopQueryHandler(IShopRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ShopModel> Handle(GetShopQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeDisplaySequenceAsync(request.Id);
            return _mapper.Map<ShopModel>(result);
        }
    }
}
