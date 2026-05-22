using Identity.Api.Features.Authentication.Commands.ChangePassword;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.ChangePassword
{
    [TestFixture]
    public class ChangePasswordCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new ChangePasswordCommand();

            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Model_SetViaProperty_ReturnsAssignedValue()
        {
            var model = new ChangePasswordModel
            {
                UserId = "user-id",
                CurrentPassword = "OldPass123!",
                NewPassword = "NewPass123!",
                ConfirmPassword = "NewPass123!"
            };
            var command = new ChangePasswordCommand { Model = model };

            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}
