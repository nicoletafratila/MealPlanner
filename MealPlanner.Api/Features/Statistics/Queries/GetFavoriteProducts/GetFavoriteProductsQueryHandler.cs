using System.Net.Http.Headers;
using Common.Api;
using Common.Constants;
using Common.Data.Entities;
using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQueryHandler(IMealPlanRepository mealPlanRepository, IServiceProvider serviceProvider) : IRequestHandler<GetFavoriteProductsQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;
        private readonly IApiConfig _recipeApiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.RecipeBook);

        public async Task<IList<StatisticModel>> Handle(GetFavoriteProductsQuery request, CancellationToken cancellationToken)
        {
            IList<ProductCategoryModel>? categories = new List<ProductCategoryModel>();
            using (var client = new HttpClient())
            {
                client.BaseAddress = _recipeApiConfig!.BaseUrl;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                categories = await client.GetFromJsonAsync<IList<ProductCategoryModel>>($"{_recipeApiConfig!.Endpoints![ApiEndpointNames.ProductCategoryApi]}", cancellationToken: cancellationToken);
            }

            var result = new List<StatisticModel>();
            foreach (var category in categories!)
            {
                var model = new StatisticModel();
                model.Title = "Favorite " + category.Name;
                model.Label = category.Name;

                IEnumerable<Product?> products = new List<Product>();
                var mealPlanWithProducts = await _mealPlanRepository.SearchByProductCategoryId(category.Id);
                foreach (var mealPlan in mealPlanWithProducts!)
                {
                    foreach (var recipe in mealPlan.MealPlanRecipes!)
                    {
                        products = products.Concat(recipe.Recipe!.RecipeIngredients!.Where(x => x.Product!.ProductCategoryId == category.Id).Select(x => x.Product));
                    }
                }

                foreach (var product in products)
                {
                    if (model.Data!.ContainsKey(product!.Name!))
                        model.Data[product.Name!]++;
                    else
                        model.Data[product.Name!] = 1;
                }
                model.Data = model.Data!.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                result.Add(model);
            }

            return result;
        }
    }
}
