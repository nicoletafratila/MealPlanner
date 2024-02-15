using FluentValidation;

namespace MealPlanner.Api.Features.Statistics.Queries.GetFavoriteProducts
{
    public class GetFavoriteProductsQueryValidator : AbstractValidator<GetFavoriteProductsQuery>
    {
        public GetFavoriteProductsQueryValidator()
        {
            RuleFor(x => x.Categories).NotEmpty().NotNull();
        }
    }
}
