using FluentValidation;

namespace RecipeBook.Api.Features.Unit.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for retrieving a unit for editing.
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