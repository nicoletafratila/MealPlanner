using FluentValidation;
using RecipeBook.Api.Features.Product.Resources;

namespace RecipeBook.Api.Features.Product.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for products.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(ProductMessages.IdGreaterThanZero);
        }
    }
}