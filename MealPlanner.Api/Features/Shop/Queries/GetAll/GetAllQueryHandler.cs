using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetAll
{
    public class GetAllQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetAllQuery, IList<ShopModel>>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<ShopModel>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var data = await _repository.GetAllAsync();
            var results = _mapper.Map<IList<ShopModel>>(data).OrderBy(r => r.Name).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
