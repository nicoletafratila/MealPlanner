using FluentValidation;

namespace MealPlanner.Api.Features.MealPlan.Queries.SearchMealPlans
{
    public class SearchMealPlansQueryValidator : AbstractValidator<SearchMealPlansQuery>
    {
        public SearchMealPlansQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
