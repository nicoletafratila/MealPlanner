using FluentValidation;

namespace Identity.Api.Features.Authentication.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Model).NotEmpty();
        }
    }
}
