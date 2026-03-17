using Identity.Api.Features.Authentication.Commands.Login;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.Authentication.Commands.Login
{
    [TestFixture]
    public class LoginCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            var command = new LoginCommand();
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            var model = new LoginModel { Username = "user", Password = "pwd" };

            var command = new LoginCommand(model);

            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new LoginCommand(null!);
            });
        }
    }
}