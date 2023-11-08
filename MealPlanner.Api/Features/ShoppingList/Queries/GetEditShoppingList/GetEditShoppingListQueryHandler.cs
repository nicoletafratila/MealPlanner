using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEditShoppingList
{
    public class GetEditShoppingListQueryHandler : IRequestHandler<GetEditShoppingListQuery, EditShoppingListModel>
    {
        private readonly IShoppingListRepository _repository;
        private readonly IMapper _mapper;

        public GetEditShoppingListQueryHandler(IShoppingListRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EditShoppingListModel> Handle(GetEditShoppingListQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeProductsAsync(request.Id);
            return _mapper.Map<EditShoppingListModel>(result);
        }
    }
}
