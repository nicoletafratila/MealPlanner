using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetUnit
{
    public class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, IList<UnitModel>>
    {
        private readonly IUnitRepository _repository;
        private readonly IMapper _mapper;

        public GetUnitQueryHandler(IUnitRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IList<UnitModel>> Handle(GetUnitQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<UnitModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
