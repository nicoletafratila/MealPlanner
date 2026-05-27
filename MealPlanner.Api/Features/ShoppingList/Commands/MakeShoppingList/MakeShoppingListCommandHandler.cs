using AutoMapper;
using Common.Services;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    /// <summary>
    /// Handles generating and persisting a shopping list from a meal plan for a given shop.
    /// </summary>
    public class MakeShoppingListCommandHandler(
        IMealPlanRepository mealPlanRepository,
        IShoppingListRepository shoppingListRepository,
        IShopRepository shopRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        ILogger<MakeShoppingListCommandHandler> logger) : IRequestHandler<MakeShoppingListCommand, ShoppingListEditModel?>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository ?? throw new ArgumentNullException(nameof(mealPlanRepository));
        private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository ?? throw new ArgumentNullException(nameof(shoppingListRepository));
        private readonly IShopRepository _shopRepository = shopRepository ?? throw new ArgumentNullException(nameof(shopRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ICurrentUserService _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        private readonly ILogger<MakeShoppingListCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<ShoppingListEditModel?> Handle(MakeShoppingListCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                    return null;

                var mealPlan = await _mealPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId, cancellationToken);
                if (mealPlan is null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId, cancellationToken);
                if (shop is null)
                    return null;

                var newList = mealPlan.MakeShoppingList(shop);
                newList.UserId = userId;

                var saved = await _shoppingListRepository.AddAsync(newList, cancellationToken);

                var ingredientsByProductId = (mealPlan.MealPlanRecipes ?? [])
                    .SelectMany(mpr => mpr.Recipe?.RecipeIngredients ?? [])
                    .Where(i => i.Product != null)
                    .GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var slProduct in saved.Products ?? [])
                {
                    if (ingredientsByProductId.TryGetValue(slProduct.ProductId, out var ingredient))
                    {
                        slProduct.Product = ingredient.Product;
                        slProduct.Unit = ingredient.Product!.BaseUnit;
                    }
                }

                return _mapper.Map<ShoppingListEditModel>(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error while making shopping list from MealPlanId {MealPlanId} for ShopId {ShopId}.",
                    request.MealPlanId, request.ShopId);

                return null;
            }
        }
    }
}