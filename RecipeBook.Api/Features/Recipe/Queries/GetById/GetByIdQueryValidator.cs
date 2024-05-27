using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    public class GetByIdQueryValidator : AbstractValidator<GetByIdQuery>
    {
        public GetByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotNull().GreaterThan(0);
        }
    }
}
