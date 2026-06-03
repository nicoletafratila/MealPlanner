using FluentValidation;
using RecipeBook.Api.Features.RecipeCategory.Resources;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for recipe categories.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeCategoryMessages.IdGreaterThanZero);
        }
    }
}