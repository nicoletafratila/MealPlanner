using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryHandler(IMealPlanRepository mealPlanRepository, IShopRepository shopRepository, IMapper mapper, ILogger<GetShoppingListProductsQueryHandler> logger) : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductEditModel>?>
    {
        public async Task<IList<ShoppingListProductEditModel>?> Handle(GetShoppingListProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await mealPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shop = await shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId);
                if (shop == null)
                    return null;

                var data = mealPlan.MakeShoppingList(shop!).Products;
                var results = data!.Select(mapper.Map<ShoppingListProductEditModel>)
                         .OrderBy(item => item.Collected)
                         .ThenBy(item => item.DisplaySequence)
                         .ThenBy(item => item.Product?.Name).ToList();
                results.SetIndexes();
                return results;
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
