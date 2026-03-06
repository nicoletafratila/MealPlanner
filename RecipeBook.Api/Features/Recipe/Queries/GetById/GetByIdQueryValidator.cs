using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Queries.GetById
{
    /// <summary>
    /// Validates GetByIdQuery for recipes.
    /// </summary>
    public class GetByIdQueryValidator : AbstractValidator<GetByIdQuery>
    {
        public GetByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id must be greater than zero.");
        }
    }
}