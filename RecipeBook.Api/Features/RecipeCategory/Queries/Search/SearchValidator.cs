using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.Search
{
    public class SearchValidator : AbstractValidator<SearchQuery>
    {
        public SearchValidator()
        {
            RuleFor(x => x.QueryParameters).NotNull();
        }
    }
}
