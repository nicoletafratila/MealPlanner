using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetEdit
{
    public class GetEditQueryValidator : AbstractValidator<GetEditMealPlanQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
