using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    public class GetEditQueryHandler : IRequestHandler<GetEditQuery, ShoppingListEditModel>
    {
        private readonly IShoppingListRepository _repository;
        private readonly IMapper _mapper;

        public GetEditQueryHandler(IShoppingListRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ShoppingListEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await _repository.GetByIdIncludeProductsAsync(request.Id);
            return _mapper.Map<ShoppingListEditModel>(result);
        }
    }
}
