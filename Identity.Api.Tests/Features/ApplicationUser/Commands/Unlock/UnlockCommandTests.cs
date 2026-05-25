using Identity.Api.Features.ApplicationUser.Commands.Unlock;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Unlock
{
    [TestFixture]
    public class UnlockCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesUserIdToNull()
        {
            var command = new UnlockCommand();

            Assert.That(command.UserId, Is.Null);
        }

        [Test]
        public void UserId_CanBeSetAndRead()
        {
            var command = new UnlockCommand { UserId = "user-42" };

            Assert.That(command.UserId, Is.EqualTo("user-42"));
        }

        [Test]
        public void UserId_CanBeUpdated()
        {
            var command = new UnlockCommand { UserId = "original" };
            command.UserId = "updated";

            Assert.That(command.UserId, Is.EqualTo("updated"));
        }
    }
}
