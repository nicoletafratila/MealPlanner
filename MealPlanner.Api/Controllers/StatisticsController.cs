using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController(ISender mediator) : ControllerBase
    {
        [HttpGet("favoriterecipes")]
        public async Task<IList<StatisticModel>> SearchFavoriteRecipesAsync([FromQuery] string? categories)
        {
            Features.Statistics.Queries.SearchRecipes.SearchQuery query = new()
            {
                Categories = new List<RecipeCategoryModel>()
            };
            foreach (var item in categories?.Split(",")!)
            {
                var category = item.Split('|');
                query.Categories?.Add(new RecipeCategoryModel { Id = int.Parse(category[0]), Name = category[1] });
            }

            return await mediator.Send(query);
        }

        [HttpGet("favoriteproducts")]
        public async Task<IList<StatisticModel>?> SearchFavoriteProductsAsync([FromQuery] string? categories)
        {
            Features.Statistics.Queries.SearchProducts.SearchQuery query = new()
            {
                Categories = new List<ProductCategoryModel>()
            };

            if (categories != null)
            {
                foreach (var item in categories!.Split(",")!)
                {
                    var category = item.Split('|');
                    query.Categories?.Add(new ProductCategoryModel { Id = int.Parse(category[0]), Name = category[1] });
                }
            }
            return await mediator.Send(query);
        }
    }
}
