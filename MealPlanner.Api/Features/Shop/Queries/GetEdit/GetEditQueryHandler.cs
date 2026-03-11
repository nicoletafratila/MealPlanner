using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a shop for editing, including its display sequence.
    /// </summary>
    public class GetEditQueryHandler(IShopRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ShopEditModel>
    {
        private readonly IShopRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<ShopEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdIncludeDisplaySequenceAsync(request.Id, cancellationToken);

            if (entity is null)
            {
                return new ShopEditModel { Id = request.Id };
            }

            var model = _mapper.Map<ShopEditModel>(entity);
            return model ?? new ShopEditModel { Id = request.Id };
        }
    }
}