using FluentValidation;
using RecipeBook.Api.Features.ProductCategory.Resources;

namespace RecipeBook.Api.Features.ProductCategory.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for product categories.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage(ProductCategoryMessages.IdGreaterThanZero);
        }
    }
}