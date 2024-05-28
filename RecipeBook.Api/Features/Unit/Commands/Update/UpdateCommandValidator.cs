using FluentValidation;

namespace RecipeBook.Api.Features.Unit.Commands.Update
{
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
