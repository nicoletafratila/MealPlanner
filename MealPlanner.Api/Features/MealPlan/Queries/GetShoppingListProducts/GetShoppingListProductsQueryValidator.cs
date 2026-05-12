using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetShoppingListProducts
{
    /// <summary>
    /// Validates GetShoppingListProductsQuery.
    /// </summary>
    public class GetShoppingListProductsQueryValidator : AbstractValidator<GetShoppingListProductsQuery>
    {
        public GetShoppingListProductsQueryValidator()
        {
            RuleFor(x => x.MealPlanId)
                .GreaterThan(0)
                .WithMessage(Resources.MealPlanMessages.MealPlanIdGreaterThanZero);

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage(Resources.MealPlanMessages.ShopIdGreaterThanZero);
        }
    }
}