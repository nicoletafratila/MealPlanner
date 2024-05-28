using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.Delete
{
    public class DeleteCommandValidator : AbstractValidator<DeleteCommand>
    {
        public DeleteCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
