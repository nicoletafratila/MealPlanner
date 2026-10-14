using Identity.Api.Features.ApplicationUser.Commands.ResendConfirmationEmail;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.ResendConfirmationEmail
{
    [TestFixture]
    public class ResendConfirmationEmailCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesUserIdToNull()
        {
            var command = new ResendConfirmationEmailCommand();

            Assert.That(command.UserId, Is.Null);
        }

        [Test]
        public void UserId_CanBeSetAndRead()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "user-42" };

            Assert.That(command.UserId, Is.EqualTo("user-42"));
        }

        [Test]
        public void UserId_CanBeUpdated()
        {
            var command = new ResendConfirmationEmailCommand { UserId = "original" };
            command.UserId = "updated";

            Assert.That(command.UserId, Is.EqualTo("updated"));
        }
    }
}
