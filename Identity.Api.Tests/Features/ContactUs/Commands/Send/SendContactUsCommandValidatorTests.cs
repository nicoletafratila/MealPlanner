using FluentValidation.TestHelper;
using Identity.Api.Features.ContactUs.Commands.Send;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.ContactUs.Commands.Send
{
    [TestFixture]
    public class SendContactUsCommandValidatorTests
    {
        private SendContactUsCommandValidator _validator = null!;

        [SetUp]
        public void SetUp()
        {
            _validator = new SendContactUsCommandValidator();
        }

        [Test]
        public void Model_Null_HasValidationError()
        {
            var command = new SendContactUsCommand { Model = null };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model);
        }

        [Test]
        public void Name_Empty_HasValidationError()
        {
            var command = BuildCommand(name: "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Name);
        }

        [Test]
        public void EmailAddress_Empty_HasValidationError()
        {
            var command = BuildCommand(email: "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void EmailAddress_InvalidFormat_HasValidationError()
        {
            var command = BuildCommand(email: "not-an-email");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.EmailAddress);
        }

        [Test]
        public void Subject_Empty_HasValidationError()
        {
            var command = BuildCommand(subject: "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Subject);
        }

        [Test]
        public void Message_Empty_HasValidationError()
        {
            var command = BuildCommand(message: "");

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Model!.Message);
        }

        [Test]
        public void ValidModel_HasNoValidationErrors()
        {
            var command = BuildCommand();

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        private static SendContactUsCommand BuildCommand(
            string name = "Jane Doe",
            string email = "jane@example.com",
            string subject = "Hello",
            string message = "Body") =>
            new()
            {
                Model = new ContactUsModel
                {
                    Name = name,
                    EmailAddress = email,
                    Subject = subject,
                    Message = message
                }
            };
    }
}
