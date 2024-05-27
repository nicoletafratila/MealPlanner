using FluentValidation;

namespace RecipeBook.Api.Features.Product.Queries.Search
{
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
