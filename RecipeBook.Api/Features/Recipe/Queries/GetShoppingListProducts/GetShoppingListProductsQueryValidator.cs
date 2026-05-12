using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

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
                .WithMessage(RecipeMessages.RecipeIdGreaterThanZero);

            RuleFor(x => x.ShopId)
                .GreaterThan(0)
                .WithMessage(RecipeMessages.ShopIdGreaterThanZero);
        }
    }
}