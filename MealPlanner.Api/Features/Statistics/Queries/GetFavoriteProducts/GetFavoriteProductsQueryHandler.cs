using Common.Shared;
using MealPlanner.Api.Repositories;
using MediatR;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQueryHandler(IMealPlanRepository mealPlanRepository) : IRequestHandler<GetFavoriteProductsQuery, IList<StatisticModel>>
    {
        private readonly IMealPlanRepository _mealPlanRepository = mealPlanRepository;

        public async Task<IList<StatisticModel>> Handle(GetFavoriteProductsQuery request, CancellationToken cancellationToken)
        {
            var result = new List<StatisticModel>();
            foreach (var category in request.Categories!)
            {
                var model = new StatisticModel
                {
                    Title = "Favorite " + category.Name,
                    Label = category.Name
                };

                var mealPlanWithProducts = await _mealPlanRepository.SearchByProductCategoryId(category.Id);
                foreach (var mealPlan in mealPlanWithProducts!)
                {
                    model.Data![mealPlan.Key.Name!] = !model.Data.TryGetValue(mealPlan.Key.Name!, out double value) ? 1 : ++value;
                }

                model.Data = model.Data?.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                result.Add(model);
            }

            return result;
        }
    }
}
