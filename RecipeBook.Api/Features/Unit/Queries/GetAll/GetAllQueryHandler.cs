using AutoMapper;
using Common.Models;
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
            var data = await _repository.GetAllAsync();
            var results= _mapper.Map<IList<UnitModel>>(data).OrderBy(r => r.UnitType).ThenBy(r => r.Name).ToList();
            results.SetIndexes();
            return results;
        }
    }
}
