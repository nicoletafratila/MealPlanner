using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEditShop
{
    public class GetEditShopQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetEditShopQuery, EditShopModel>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditShopModel> Handle(GetEditShopQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeDisplaySequenceAsync(request.Id);
            return _mapper.Map<EditShopModel>(result);
        }
    }
}
