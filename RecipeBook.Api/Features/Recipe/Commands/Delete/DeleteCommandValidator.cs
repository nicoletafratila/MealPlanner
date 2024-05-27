using FluentValidation;

namespace RecipeBook.Api.Features.Recipe.Commands.Delete
{
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
