using FluentValidation;

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
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}