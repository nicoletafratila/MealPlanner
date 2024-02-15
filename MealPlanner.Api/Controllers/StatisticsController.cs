using Common.Shared;
using MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts;
using MealPlanner.Api.Features.Statistics.Queries.GetFavoriteRecipes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly ISender _mediator;

        public StatisticsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("favoriterecipes")]
        public async Task<IList<StatisticModel>> SearchFavoriteRecipes([FromQuery] string? categories)
        {
            GetFavoriteRecipesQuery query = new()
            {
                Categories = new List<RecipeCategoryModel>()
            };
            foreach (var item in categories!.Split(","))
            {
                var category = item.Split('|');
                query.Categories!.Add(new RecipeCategoryModel { Id = int.Parse(category[0]), Name = category[1] });
            }
            return await _mediator.Send(query);
        }

        [HttpGet("favoriteproducts")]
        public async Task<IList<StatisticModel>?> SearchFavoriteProducts([FromQuery] string? categories)
        {
            GetFavoriteProductsQuery query = new()
            {
                Categories = new List<ProductCategoryModel>()
            };
            foreach (var item in categories!.Split(","))
            {
                var category = item.Split('|');
                query.Categories!.Add(new ProductCategoryModel { Id = int.Parse(category[0]), Name = category[1] });
            }
            return await _mediator.Send(query);
        }
    }
}
