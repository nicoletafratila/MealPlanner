using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Recipe.Commands.Share
{
    /// <summary>
    /// Validates recipe share commands.
    /// </summary>
    public class ShareCommandValidator : AbstractValidator<ShareCommand>
    {
        public ShareCommandValidator()
        {
            RuleFor(x => x.RecipeId)
                .NotEqual(Guid.Empty)
                .WithMessage(RecipeMessages.IdGreaterThanZero);

            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(RecipeMessages.UserIdRequired);
        }
    }
}
