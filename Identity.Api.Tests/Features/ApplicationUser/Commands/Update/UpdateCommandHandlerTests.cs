using AutoMapper;
using Identity.Api.Features.ApplicationUser.Commands.Update;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.ApplicationUser.Commands.Update
{
    [TestFixture]
    public class UpdateCommandHandlerTests
    {
        private Mock<UserManager<Common.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private Mock<ILogger<UpdateCommandHandler>> _loggerMock = null!;
        private UpdateCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Common.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Common.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _mapperMock = new Mock<IMapper>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<UpdateCommandHandler>>(MockBehavior.Loose);

            _handler = new UpdateCommandHandler(
                _userManagerMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Test]
        public void Ctor_NullUserManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(null!, _mapperMock.Object, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullMapper_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(_userManagerMock.Object, null!, _loggerMock.Object));
        }

        [Test]
        public void Ctor_NullLogger_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _ = new UpdateCommandHandler(_userManagerMock.Object, _mapperMock.Object, null!));
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public async Task Handle_ModelNullOrUserIdMissing_ReturnsFailedResponse()
        {
            var command1 = new UpdateCommand { Model = null! };
            var command2 = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "   ",
                    EmailAddress = "user@example.com"
                }
            };

            var result1 = await _handler.Handle(command1, CancellationToken.None);
            var result2 = await _handler.Handle(command2, CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.Not.Null);
                Assert.That(result1!.Succeeded, Is.False);
                Assert.That(result1.Message, Does.StartWith("Could not find a user with id"));

                Assert.That(result2, Is.Not.Null);
                Assert.That(result2!.Succeeded, Is.False);
                Assert.That(result2.Message, Does.StartWith("Could not find a user with id"));
            });

            _userManagerMock.Verify(
                m => m.FindByIdAsync(It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailedResponse()
        {
            // Arrange
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "123",
                    EmailAddress = "user@example.com"
                }
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("123"))
                .ReturnsAsync((Common.Data.Entities.ApplicationUser?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Could not find a user with id = 123"));
            });

            _userManagerMock.Verify(m => m.FindByIdAsync("123"), Times.Once);
            _mapperMock.Verify(m => m.Map(It.IsAny<ApplicationUserEditModel>(), It.IsAny<Common.Data.Entities.ApplicationUser>()), Times.Never);
        }

        [Test]
        public async Task Handle_UpdateFails_ReturnsFailedWithErrors()
        {
            // Arrange
            var existing = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "1",
                    EmailAddress = "bad-email"
                }
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(command.Model, existing))
                .Returns(existing);

            var identityResult = IdentityResult.Failed(
                new IdentityError { Description = "Invalid email" },
                new IdentityError { Description = "Another error" });

            _userManagerMock
                .Setup(m => m.UpdateAsync(existing))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Invalid email; Another error"));
            });

            _userManagerMock.Verify(m => m.FindByIdAsync("1"), Times.Once);
            _mapperMock.Verify(m => m.Map(command.Model, existing), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(existing), Times.Once);
        }

        [Test]
        public async Task Handle_SuccessfulUpdate_ReturnsSuccess()
        {
            // Arrange
            var existing = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "1",
                    EmailAddress = "user@example.com"
                }
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(command.Model, existing))
                .Returns(existing);

            _userManagerMock
                .Setup(m => m.UpdateAsync(existing))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Succeeded, Is.True);

            _userManagerMock.Verify(m => m.FindByIdAsync("1"), Times.Once);
            _mapperMock.Verify(m => m.Map(command.Model, existing), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(existing), Times.Once);
        }

        [Test]
        public async Task Handle_Exception_LogsError_AndReturnsFailedResponse()
        {
            // Arrange
            var existing = new Common.Data.Entities.ApplicationUser { Id = "1", UserName = "user" };
            var command = new UpdateCommand
            {
                Model = new ApplicationUserEditModel
                {
                    UserId = "1",
                    EmailAddress = "user@example.com"
                }
            };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("1"))
                .ReturnsAsync(existing);

            _mapperMock
                .Setup(m => m.Map(command.Model, existing))
                .Returns(existing);

            _userManagerMock
                .Setup(m => m.UpdateAsync(existing))
                .ThrowsAsync(new InvalidOperationException("DB error"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("An error occurred while updating the user."));
            });

            _userManagerMock.Verify(m => m.FindByIdAsync("1"), Times.Once);
            _mapperMock.Verify(m => m.Map(command.Model, existing), Times.Once);
            _userManagerMock.Verify(m => m.UpdateAsync(existing), Times.Once);

            _loggerMock.Verify(
                l => l.Log(
                    It.Is<LogLevel>(ll => ll == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}