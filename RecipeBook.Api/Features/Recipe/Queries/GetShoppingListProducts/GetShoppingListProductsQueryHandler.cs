﻿using System.Net.Http.Headers;
using AutoMapper;
using Common.Api;
using Common.Constants;
using Common.Data.Entities;
using Common.Models;
using MealPlanner.Shared.Models;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryHandler(IRecipeRepository recipeRepository, IServiceProvider serviceProvider, IMapper mapper, ILogger<GetShoppingListProductsQueryHandler> logger) : IRequestHandler<GetShoppingListProductsQuery, IList<ShoppingListProductEditModel>?>
    {
        private readonly IRecipeRepository _recipeRepository = recipeRepository;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<GetShoppingListProductsQueryHandler> _logger = logger;
        private readonly IApiConfig _mealPlannerApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<IList<ShoppingListProductEditModel>?> Handle(GetShoppingListProductsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var recipe = await _recipeRepository.GetByIdIncludeIngredientsAsync(request.RecipeId);
                if (recipe == null)
                    return null;

                var shop = new ShopEditModel();
                using (var client = new HttpClient())
                {
                    client.BaseAddress = _mealPlannerApiConfig?.BaseUrl;
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    shop = await client.GetFromJsonAsync<ShopEditModel>($"{_mealPlannerApiConfig?.Endpoints![ApiEndpointNames.ShopApi]}/edit/{request.ShopId}", cancellationToken);
                    if (shop == null)
                        return null;
                }

                var data = recipe.MakeShoppingList(_mapper.Map<Shop>(shop)!).Products;
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
