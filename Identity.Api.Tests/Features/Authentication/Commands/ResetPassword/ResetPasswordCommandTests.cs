using Identity.Api.Features.Authentication.Commands.ResetPassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ResetPassword
{
    [TestFixture]
    public class ResetPasswordCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new ResetPasswordCommand();

            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Model_SetViaProperty_ReturnsAssignedValue()
        {
            var model = new ResetPasswordModel { UserId = "uid", Token = "tok", NewPassword = "pwd", ConfirmPassword = "pwd" };
            var command = new ResetPasswordCommand { Model = model };

            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}
