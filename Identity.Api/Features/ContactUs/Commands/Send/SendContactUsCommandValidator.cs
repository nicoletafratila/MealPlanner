using FluentValidation;
using Identity.Api.Features.ContactUs.Resources;

namespace Identity.Api.Features.ContactUs.Commands.Send
{
    public class SendContactUsCommandValidator : AbstractValidator<SendContactUsCommand>
    {
        public SendContactUsCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .WithMessage(ContactUsMessages.ModelRequired)
                .DependentRules(() =>
                {
                    RuleFor(x => x.Model!.Name)
                        .NotEmpty()
                        .WithMessage(ContactUsMessages.NameRequired);

                    RuleFor(x => x.Model!.EmailAddress)
                        .NotEmpty()
                        .WithMessage(ContactUsMessages.EmailRequired)
                        .EmailAddress()
                        .WithMessage(ContactUsMessages.EmailInvalid);

                    RuleFor(x => x.Model!.Subject)
                        .NotEmpty()
                        .WithMessage(ContactUsMessages.SubjectRequired);

                    RuleFor(x => x.Model!.Message)
                        .NotEmpty()
                        .WithMessage(ContactUsMessages.MessageRequired);
                });
        }
    }
}
