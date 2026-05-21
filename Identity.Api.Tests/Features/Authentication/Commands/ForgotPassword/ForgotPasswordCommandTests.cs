using Identity.Api.Features.Authentication.Commands.ForgotPassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ForgotPassword
{
    [TestFixture]
    public class ForgotPasswordCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new ForgotPasswordCommand();

            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Model_SetViaProperty_ReturnsAssignedValue()
        {
            var model = new ForgotPasswordModel { EmailAddress = "user@example.com" };
            var command = new ForgotPasswordCommand { Model = model };

            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}
