using AutoMapper;
using MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.AddMealPlanToShoppingList
{
    public class AddMealPlanToShoppingListCommandHandler(IMealPlanRepository mealPlanRepository, IShoppingListRepository shoppingListRepository, IShopRepository shopRepository, IMapper mapper, ILogger<AddMealPlanCommandHandler> logger) : IRequestHandler<AddMealPlanToShoppingListCommand, EditShoppingListModel?>
    {
        private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
        private readonly IMealPlanRepository _meanPlanRepository = mealPlanRepository;
        private readonly IShopRepository _shopRepository = shopRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AddMealPlanCommandHandler> _logger = logger;

        public async Task<EditShoppingListModel?> Handle(AddMealPlanToShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shoppingList = await _shoppingListRepository.GetByIdIncludeProductsAsync(request.ShoppingListId);
                if (shoppingList == null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(shoppingList.ShopId);
                if (shop == null) 
                    return null;

                var listFromMealPlan = mealPlan.MakeShoppingList(shop);
                foreach (var item in listFromMealPlan.Products!)
                {
                    var existingProduct = shoppingList.Products!.FirstOrDefault(x => x.ProductId == item.ProductId);
                    if (existingProduct == null)
                        shoppingList.Products!.Add(item);
                    else
                        existingProduct.Quantity += item.Quantity;
                }
                await _shoppingListRepository.UpdateAsync(shoppingList);

                var result = await _shoppingListRepository.GetByIdIncludeProductsAsync(request.ShoppingListId);
                return _mapper.Map<EditShoppingListModel>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
