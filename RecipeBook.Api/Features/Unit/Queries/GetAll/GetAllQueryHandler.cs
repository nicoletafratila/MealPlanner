using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetAll
{
    public class GetAllQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<GetAllQuery, IList<UnitModel>>
    {
        private readonly IUnitRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<IList<UnitModel>> Handle(GetAllQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetAllAsync();
            return _mapper.Map<IList<UnitModel>>(results).OrderBy(r => r.Name).ToList();
        }
    }
}
