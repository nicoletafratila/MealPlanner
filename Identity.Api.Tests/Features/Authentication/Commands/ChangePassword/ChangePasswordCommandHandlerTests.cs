using Identity.Api.Features.Authentication.Commands.ChangePassword;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.ChangePassword
{
    [TestFixture]
    public class ChangePasswordCommandHandlerTests
    {
        private Mock<UserManager<Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<ILogger<ChangePasswordCommandHandler>> _loggerMock = null!;
        private ChangePasswordCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<ChangePasswordCommandHandler>>(MockBehavior.Loose);

            _handler = new ChangePasswordCommandHandler(_userManagerMock.Object, _loggerMock.Object);
        }

        [Test]
        public void Handle_NullRequest_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(null!, CancellationToken.None));
        }

        [Test]
        public void Handle_NullModel_ThrowsArgumentNullException()
        {
            var command = new ChangePasswordCommand { Model = null };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailedResponse()
        {
            _userManagerMock
                .Setup(m => m.FindByIdAsync("unknown-id"))
                .ReturnsAsync((Data.Entities.ApplicationUser?)null);

            var result = await _handler.Handle(BuildCommand(userId: "unknown-id"), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.Not.Null.And.Not.Empty);
            }

            _userManagerMock.Verify(
                m => m.ChangePasswordAsync(
                    It.IsAny<Data.Entities.ApplicationUser>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_ChangePasswordFails_ReturnsFailedResponseWithIdentityErrors()
        {
            var user = new Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ChangePasswordAsync(user, "wrong-current", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Incorrect password." }));

            var result = await _handler.Handle(BuildCommand(currentPassword: "wrong-current"), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Does.Contain("Incorrect password."));
            }
        }

        [Test]
        public async Task Handle_Success_ReturnsSuccessResponse()
        {
            var user = new Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ChangePasswordAsync(user, "OldPass123!", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public async Task Handle_Success_CallsChangePasswordWithCorrectArguments()
        {
            var user = new Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ChangePasswordAsync(user, "OldPass123!", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(BuildCommand(), CancellationToken.None);

            _userManagerMock.Verify(
                m => m.ChangePasswordAsync(user, "OldPass123!", "NewPass123!"),
                Times.Once);
        }

        private static ChangePasswordCommand BuildCommand(
            string userId = "user-id",
            string currentPassword = "OldPass123!",
            string newPassword = "NewPass123!") =>
            new()
            {
                Model = new ChangePasswordModel
                {
                    UserId = userId,
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                }
            };
    }
}
