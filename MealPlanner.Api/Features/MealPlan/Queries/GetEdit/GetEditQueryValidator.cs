using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    /// <summary>
    /// Validates GetEditMealPlanQuery.
    /// </summary>
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEqual(Guid.Empty)
                .WithMessage(Resources.MealPlanMessages.IdGreaterThanZero);
        }
    }
}