using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{
    public class GetEditQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, EditUnitModel>
    {
        private readonly IUnitRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        public async Task<EditUnitModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdAsync(request.Id);
            return _mapper.Map<EditUnitModel>(result);
        }
    }
}
