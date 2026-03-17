using FluentValidation;

namespace Identity.Api.Features.Authentication.Commands.Logout
{
    /// <summary>
    /// Validator for LogoutCommand. 
    /// Currently no additional validation is needed.
    /// </summary>
    public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        public LogoutCommandValidator()
        {
        }
    }
}