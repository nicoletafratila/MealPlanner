using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    /// <summary>
    /// Handles generating shopping-list product view models from a meal plan for a specific shop.
    /// </summary>
    public class GetShoppingListProductsQueryHandler(
        IMealPlanRepository mealPlanRepository,
        IShopRepository shopRepository,
        IMapper mapper,
        ILogger<GetShoppingListProductsQueryHandler> logger) : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductEditModel>?>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository ?? throw new ArgumentNullException(nameof(mealPlanRepository));
        private readonly IShopRepository _shopRepository = shopRepository ?? throw new ArgumentNullException(nameof(shopRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetShoppingListProductsQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IList<ShoppingListProductEditModel>?> Handle(GetShoppingListProductsQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var mealPlan = await _mealPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId, cancellationToken);
                if (mealPlan is null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId, cancellationToken);
                if (shop is null)
                    return null;

                var products = mealPlan.MakeShoppingList(shop).Products;
                if (products is null)
                    return Array.Empty<ShoppingListProductEditModel>();

                var results = products
                    .Select(_mapper.Map<ShoppingListProductEditModel>)
                    .OrderBy(item => item.Collected)
                    .ThenBy(item => item.DisplaySequence)
                    .ThenBy(item => item.Product?.Name)
                    .ToList();

                results.SetIndexes();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An error occurred while getting shopping list products for MealPlanId {MealPlanId} and ShopId {ShopId}.",
                    request.MealPlanId, request.ShopId);
                return null;
            }
        }
    }
}