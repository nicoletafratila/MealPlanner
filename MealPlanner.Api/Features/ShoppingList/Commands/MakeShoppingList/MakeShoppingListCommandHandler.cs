using AutoMapper;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    public class MakeShoppingListCommandHandler(IMealPlanRepository mealPlanRepository, IShoppingListRepository shoppingListRepository, IShopRepository shopRepository, IMapper mapper, ILogger<MakeShoppingListCommandHandler> logger) : IRequestHandler<MakeShoppingListCommand, ShoppingListEditModel?>
    {
        private readonly IShoppingListRepository _shoppingListRepository = shoppingListRepository;
        private readonly IMealPlanRepository _meanPlanRepository = mealPlanRepository;
        private readonly IShopRepository _shopRepository = shopRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<MakeShoppingListCommandHandler> _logger = logger;

        public async Task<ShoppingListEditModel?> Handle(MakeShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId);
                var data = await _shoppingListRepository.AddAsync(mealPlan.MakeShoppingList(shop!));
                var list = await _shoppingListRepository.GetByIdIncludeProductsAsync(data.Id);
                return _mapper.Map<ShoppingListEditModel>(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
