using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryValidator : AbstractValidator<GetShoppingListProductsQuery>
    {
        public GetShoppingListProductsQueryValidator()
        {
            RuleFor(x => x.MealPlanId).NotNull().GreaterThan(0);
            RuleFor(x => x.ShopId).NotNull().GreaterThan(0);
        }
    }
}
