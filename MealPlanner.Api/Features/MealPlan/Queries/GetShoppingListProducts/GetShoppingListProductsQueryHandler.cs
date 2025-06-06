﻿using AutoMapper;
using Common.Models;
using MealPlanner.Api.Repositories;
using MealPlanner.Shared.Models;
using MediatR;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryHandler(IMealPlanRepository mealPlanRepository, IShopRepository shopRepository, IMapper mapper, ILogger<GetShoppingListProductsQueryHandler> logger) : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductEditModel>?>
    {
        private readonly IMealPlanRepository _meanPlanRepository = mealPlanRepository;
        private readonly IShopRepository _shopRepository = shopRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetShoppingListProductsQueryHandler> _logger = logger;
        public async Task<IList<ShoppingListProductEditModel>?> Handle(GetShoppingListProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var mealPlan = await _meanPlanRepository.GetByIdIncludeRecipesAsync(request.MealPlanId);
                if (mealPlan == null)
                    return null;

                var shop = await _shopRepository.GetByIdIncludeDisplaySequenceAsync(request.ShopId);
                if (shop == null)
                    return null;

                var data = mealPlan.MakeShoppingList(shop!).Products;
                var results = data!.Select(_mapper.Map<ShoppingListProductEditModel>)
                         .OrderBy(item => item.Collected)
                         .ThenBy(item => item.DisplaySequence)
                         .ThenBy(item => item.Product?.Name).ToList();
                results.SetIndexes();
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }
    }
}
