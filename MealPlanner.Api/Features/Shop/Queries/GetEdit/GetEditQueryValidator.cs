using FluentValidation;

namespace MealPlanner.Api.Features.Shop.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditQuery for shops.
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