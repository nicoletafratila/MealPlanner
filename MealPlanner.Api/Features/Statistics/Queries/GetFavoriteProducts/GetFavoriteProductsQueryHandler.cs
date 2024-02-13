using Common.Data.Entities;
using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<GetFavoriteProductsQuery, StatisticModel?>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<StatisticModel?> Handle(GetFavoriteProductsQuery request, CancellationToken cancellationToken)
        {
            var categoryId = int.Parse(request.CategoryId!);
            var model = new StatisticModel()
            {
                Data = new Dictionary<string, double>()
            };

            IEnumerable<Product?> products = new List<Product>();
            var mealPlanWithProducts = await _mealPlanRepository.SearchByProductCategoryId(categoryId);
            foreach (var mealPlan in mealPlanWithProducts!)
            {
                foreach (var recipe in mealPlan.MealPlanRecipes!)
                {
                    products = products.Union(recipe.Recipe!.RecipeIngredients!.Where(x => x.Product!.ProductCategoryId == categoryId).Select(x => x.Product));
                }
            }

            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(model.Label))
                {
                    model.Title = "Favorite " + product!.ProductCategory!.Name;
                    model.Label = product.ProductCategory!.Name;
                }
                if (model.Data.ContainsKey(product!.Name!))
                    model.Data[product.Name!]++;
                else
                    model.Data[product.Name!] = 1;
            }
            model.Data = model.Data.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return model;
        }
    }
}
