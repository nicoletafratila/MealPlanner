using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Queries.GetShop
{
    public class GetShopQueryValidator : AbstractValidator<GetShopQuery>
    {
        public GetShopQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
