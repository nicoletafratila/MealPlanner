using FluentValidation;

namespace MealPlanner.Api.Features.Statistics.Queries.SearchProducts
{
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.Categories).NotEmpty().NotNull();
        }
    }
}
