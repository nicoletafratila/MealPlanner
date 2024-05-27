using FluentValidation;

namespace RecipeBook.Api.Features.ProductCategory.Queries.Search
{
    public class SearchQueryValidator : AbstractValidator<SearchQuery>
    {
        public SearchQueryValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
