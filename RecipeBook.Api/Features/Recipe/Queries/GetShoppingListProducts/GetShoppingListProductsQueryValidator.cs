using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    /// <summary>
    /// Validates GetShoppingListProductsQuery.
    /// </summary>
    public class GetShoppingListProductsQueryValidator : AbstractValidator<GetShoppingListProductsQuery>
    {
        public GetShoppingListProductsQueryValidator()
        {
            RuleFor(x => x.RecipeId)
                .GreaterThan(0)
                .WithMessage("RecipeId must be greater than zero.");

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage("ShopId must be greater than zero.");
        }
    }
}