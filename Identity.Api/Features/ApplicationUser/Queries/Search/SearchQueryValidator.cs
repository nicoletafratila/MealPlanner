using FluentValidation;

namespace Identity.Api.Features.ApplicationUser.Queries.Search
{
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters)
                .NotNull()
                .WithMessage(Resources.ApplicationUserMessages.QueryParametersRequired);
        }
    }
}
