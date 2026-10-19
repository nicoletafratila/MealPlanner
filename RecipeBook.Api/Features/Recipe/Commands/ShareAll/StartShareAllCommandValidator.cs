using FluentValidation;
using RecipeBook.Api.Features.Recipe.Resources;

namespace RecipeBook.Api.Features.Recipe.Commands.ShareAll
{
    /// <summary>
    /// Validates start-share-all-recipes commands.
    /// </summary>
    public class StartShareAllCommandValidator : AbstractValidator<StartShareAllCommand>
    {
        public StartShareAllCommandValidator()
        {
            RuleFor(x => x.TargetUserId)
                .NotEmpty()
                .WithMessage(RecipeMessages.UserIdRequired);
        }
    }
}
