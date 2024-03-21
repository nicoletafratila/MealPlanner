using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetShoppingListProducts
{
    public class GetShoppingListProductsQueryValidator : AbstractValidator<GetShoppingListProductsQuery>
    {
        public GetShoppingListProductsQueryValidator()
        {
            RuleFor(x => x.RecipeId).NotNull().GreaterThan(0);
            RuleFor(x => x.ShopId).NotNull().GreaterThan(0);
        }
    }
}
