using AutoMapper;
using MediatR;
using RecipeBook.Api.Repositories;
using RecipeBook.Shared.Models;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{
    /// <summary>
    /// Handles requests to retrieve a unit for editing.
    /// </summary>
    public class GetEditQueryHandler(IUnitRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, UnitEditModel>
    {
        private readonly IUnitRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<UnitEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
                return new UnitEditModel { Id = request.Id };

            var model = _mapper.Map<UnitEditModel>(entity);
            return model ?? new UnitEditModel { Id = request.Id };
        }
    }
}