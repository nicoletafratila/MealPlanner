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
                .WithMessage("MealPlanId must be greater than zero.");

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage("ShopId must be greater than zero.");
        }
    }
}