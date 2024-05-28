using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    public class GetEditQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, EditShopModel>
    {
        private readonly IShopRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditShopModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeDisplaySequenceAsync(request.Id);
            return _mapper.Map<EditShopModel>(result);
        }
    }
}
