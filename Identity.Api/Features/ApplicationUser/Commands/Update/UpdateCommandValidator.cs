using FluentValidation;

namespace Identity.Api.Features.ApplicationUser.Commands.Update
{
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
