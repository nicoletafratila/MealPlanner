using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{
    public class GetEditQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, UnitEditModel>
    {
        public async Task<UnitEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdAsync(request.Id);
            return mapper.Map<UnitEditModel>(result);
        }
    }
}
