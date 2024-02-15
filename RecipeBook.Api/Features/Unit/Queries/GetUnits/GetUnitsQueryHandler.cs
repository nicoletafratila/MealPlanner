using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetUnits
{
    public class GetUnitsQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<GetUnitsQuery, IList<UnitModel>>
    {
        private readonly IUnitRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<UnitModel>> Handle(GetUnitsQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<UnitModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
