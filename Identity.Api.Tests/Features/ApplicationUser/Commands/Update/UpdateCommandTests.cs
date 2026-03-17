using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Shared.Models;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Update
{
    [TestFixture]
    public class UpdateCommandTests
    {
        [Test]
        public void DefaultCtor_InitializesModelToNull()
        {
            // Act
            var command = new UpdateCommand();

            // Assert
            Assert.That(command.Model, Is.Null);
        }

        [Test]
        public void Ctor_SetsModel()
        {
            // Arrange
            var model = new ApplicationUserEditModel
            {
                UserId = "1",
                Username = "user",
                EmailAddress = "user@example.com"
            };

            // Act
            var command = new UpdateCommand(model);

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }

        [Test]
        public void Ctor_NullModel_ThrowsArgumentNullException()
        {
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                _ = new UpdateCommand(null!);
            });
        }

        [Test]
        public void Can_Set_And_Get_Model()
        {
            // Arrange
            var command = new UpdateCommand();
            var model = new ApplicationUserEditModel
            {
                UserId = "2",
                Username = "other",
                EmailAddress = "other@example.com"
            };

            // Act
            command.Model = model;

            // Assert
            Assert.That(command.Model, Is.SameAs(model));
        }
    }
}