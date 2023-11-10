using AutoMapper;
using MealPlanner.Api.Features.MealPlan.Commands.AddMealPlan;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.ShoppingList.Commands.MakeShoppingList
{
    public class MakeShoppingListCommandHandler : IRequestHandler<MakeShoppingListCommand, EditShoppingListModel?>
    {
        private readonly IShoppingListRepository _shoppingListRepository;
        private readonly IMealPlanRepository _meanPlanRepository;
        private readonly IMapper _mapper; 
        private readonly ILogger<AddMealPlanCommandHandler> _logger;

        public MakeShoppingListCommandHandler(IMealPlanRepository mealPlanRepository, IShoppingListRepository shoppingListRepository, IMapper mapper, ILogger<AddMealPlanCommandHandler> logger)
        {
            _meanPlanRepository = mealPlanRepository;
            _shoppingListRepository = shoppingListRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<EditShoppingListModel?> Handle(MakeShoppingListCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var data = await _shoppingListRepository.AddAsync(mealPlan.MakeShoppingList());
                return _mapper.Map<EditShoppingListModel>(await _shoppingListRepository.GetByIdIncludeProductsAsync(data.Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
