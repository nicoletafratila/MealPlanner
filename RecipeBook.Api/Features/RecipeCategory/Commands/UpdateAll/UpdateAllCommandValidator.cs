using FluentValidation;

namespace RecipeBook.Api.Features.RecipeCategory.Commands.UpdateAll
{
    public class UpdateAllCommandValidator : AbstractValidator<UpdateAllCommand>
    {
        public UpdateAllCommandValidator()
        {
            RuleFor(x => x.Models).NotNull().NotEmpty();
        }
    }
}
