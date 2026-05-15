using Identity.Api.Features.Authentication.Commands.Register;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.Register
{
    [TestFixture]
    public class RegisterCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToDefault()
        {
            var command = new RegisterCommand();

            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            var model = new RegistrationModel { Username = "user", EmailAddress = "user@example.com" };

            var command = new RegisterCommand(model);

            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new RegisterCommand(null!);
            });
        }
    }
}
