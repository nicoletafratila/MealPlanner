using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Queries.GetEditShop
{
    public class GetEditShopQueryValidator : AbstractValidator<GetEditShopQuery>
    {
        public GetEditShopQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
