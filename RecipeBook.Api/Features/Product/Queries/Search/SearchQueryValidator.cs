using FluentValidation;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    /// <summary>
    /// Validates product search queries.
    /// </summary>
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters)
                .NotNull()
                .WithMessage("QueryParameters is required.");
        }
    }
}