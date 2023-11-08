using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.GetMealPlan
{
    public class GetEditMealPlanQueryValidator : AbstractValidator<GetEditMealPlanQuery>
    {
        public GetEditMealPlanQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
