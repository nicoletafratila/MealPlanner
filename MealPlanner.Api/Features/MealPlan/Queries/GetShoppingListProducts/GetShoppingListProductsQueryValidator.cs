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
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.MealPlanMessages.MealPlanIdGreaterThanZero);

            RuleFor(x => x.ShopId)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.MealPlanMessages.ShopIdGreaterThanZero);
        }
    }
}