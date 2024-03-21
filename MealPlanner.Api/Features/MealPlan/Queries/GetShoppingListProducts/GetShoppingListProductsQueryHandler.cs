using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryHandler(IMealPlanRepository mealPlanRepository, IShopRepository shopRepository, IMapper mapper, ILogger<GetShoppingListProductsQueryHandler> logger) : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductModel>?>
    {
        private readonly IMealPlanRepository _meanPlanRepository = mealPlanRepository;
        private readonly IShopRepository _shopRepository = shopRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetShoppingListProductsQueryHandler> _logger = logger;
        public async Task<IList<ShoppingListProductModel>?> Handle(GetShoppingListProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId);
                if (shop == null)
                    return null;

                var products = mealPlan.MakeShoppingList(shop!).Products;
                return products!.Select(_mapper.Map<ShoppingListProductModel>)
                         .OrderBy(item => item.Collected)
                         .ThenBy(item => item.DisplaySequence)
                         .ThenBy(item => item.Product?.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
