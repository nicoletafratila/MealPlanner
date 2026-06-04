using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Recipe.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for recipes.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeMessages.IdGreaterThanZero);
        }
    }
}