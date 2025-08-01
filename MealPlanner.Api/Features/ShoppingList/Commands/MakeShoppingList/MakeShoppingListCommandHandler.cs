using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    public class MakeShoppingListCommandHandler(IMealPlanRepository mealPlanRepository, IShoppingListRepository shoppingListRepository, IShopRepository shopRepository, IMapper mapper, ILogger<MakeShoppingListCommandHandler> logger) : IRequestHandler<MakeShoppingListCommand, ShoppingListEditModel?>
    {
        public async Task<ShoppingListEditModel?> Handle(MakeShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await mealPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shop = await shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId);
                var data = await shoppingListRepository.AddAsync(mealPlan.MakeShoppingList(shop!));
                var list = await shoppingListRepository.GetByIdIncludeProductsAsync(data.Id);
                return mapper.Map<ShoppingListEditModel>(list);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
