using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Job.Queries.GetStatus
{
    /// <summary>
    /// Validates GetStatusQuery.
    /// </summary>
    public class GetStatusQueryValidator : AbstractValidator<GetStatusQuery>
    {
        public GetStatusQueryValidator()
        {
            RuleFor(x => x.JobId)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeMessages.IdGreaterThanZero);
        }
    }
}
