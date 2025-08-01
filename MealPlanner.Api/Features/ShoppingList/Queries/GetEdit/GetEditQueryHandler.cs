using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Queries.GetEdit
{
    public class GetEditQueryHandler(IShoppingListRepository repository, IMapper mapper) : IRequestHandler<GetEditQuery, ShoppingListEditModel>
    {
        public async Task<ShoppingListEditModel> Handle(GetEditQuery request, CancellationToken cancellationToken)
        {
            var result = await repository.GetByIdIncludeProductsAsync(request.Id);
            return mapper.Map<ShoppingListEditModel>(result);
        }
    }
}
