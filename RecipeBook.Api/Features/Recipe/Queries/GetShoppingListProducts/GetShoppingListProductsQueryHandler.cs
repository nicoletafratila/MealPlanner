using AutoMapper;
using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Abstractions;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryHandler(
        IRecipeRepository recipeRepository,
        IMapper mapper,
        ILogger<GetShoppingListProductsQueryHandler> logger,
        IMealPlannerClient mealPlannerClient)
                : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductEditModel>?>
    {
        private readonly IRecipeRepository _recipeRepository = recipeRepository ?? throw new ArgumentNullException(nameof(recipeRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetShoppingListProductsQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMealPlannerClient _mealPlannerClient = mealPlannerClient ?? throw new ArgumentNullException(nameof(mealPlannerClient));

        public async Task<IList<ShoppingListProductEditModel>?> Handle(
            GetShoppingListProductsQuery request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                var recipe = await _recipeRepository.GetByIdIncludeIngredientsAsync(request.RecipeId, cancellationToken);
                if (recipe is null)
                    return null;

                var shop = await _mealPlannerClient.GetShopAsync(request.ShopId, request.AuthToken, cancellationToken);
                if (shop is null)
                    return null;

                var shopEntity = _mapper.Map<Common.Data.Entities.Shop>(shop);
                var shoppingList = recipe.MakeShoppingList(shopEntity);
                var products = shoppingList.Products;

                if (products is null)
                    return [];

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
                _logger.LogError(ex, "Error while getting shopping list products for RecipeId {RecipeId} and ShopId {ShopId}.", request.RecipeId, request.ShopId);
                return null;
            }
        }
    }
}