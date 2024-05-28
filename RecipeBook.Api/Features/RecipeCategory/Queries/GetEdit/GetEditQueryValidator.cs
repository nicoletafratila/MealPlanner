using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Queries.GetEdit
{
    public class GetEditQueryValidator : AbstractValidator<GetEditQuery>
    {
        public GetEditQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
