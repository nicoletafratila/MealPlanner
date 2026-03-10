using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    /// <summary>
    /// Handles retrieving a shopping list for editing, including its products.
    /// </summary>
    public class GetEditQueryHandler(IShoppingListRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ShoppingListEditModel>
    {
        private readonly IShoppingListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        public async Task<ShoppingListEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = await _repository.GetByIdIncludeProductsAsync(request.Id);

            if (entity is null)
            {
                // Return an empty edit model if not found so caller can decide how to handle it
                return new ShoppingListEditModel { Id = request.Id };
            }

            var model = _mapper.Map<ShoppingListEditModel>(entity);
            return model ?? new ShoppingListEditModel { Id = request.Id };
        }
    }
}