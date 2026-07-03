using Identity.Api.Features.Authentication.Commands.ResetPassword;
using Identity.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Identity.Api.Tests.Features.Authentication.Commands.ResetPassword
{
    [TestFixture]
    public class ResetPasswordCommandHandlerTests
    {
        private Mock<UserManager<Identity.Data.Entities.ApplicationUser>> _userManagerMock = null!;
        private Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock = null!;
        private ResetPasswordCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Identity.Data.Entities.ApplicationUser>>(
                Mock.Of<IUserStore<Identity.Data.Entities.ApplicationUser>>(),
                null, null, null, null, null, null, null, null);

            _loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>(MockBehavior.Loose);

            _handler = new ResetPasswordCommandHandler(_userManagerMock.Object, _loggerMock.Object);
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
            var command = new ResetPasswordCommand { Model = null };

            Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _handler.Handle(command, CancellationToken.None));
        }

        [Test]
        public async Task Handle_UserNotFound_ReturnsFailedResponse()
        {
            _userManagerMock
                .Setup(m => m.FindByIdAsync("unknown-id"))
                .ReturnsAsync((Identity.Data.Entities.ApplicationUser?)null);

            var result = await _handler.Handle(BuildCommand(userId: "unknown-id"), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Password reset failed. The link may be invalid or expired."));
            }

            _userManagerMock.Verify(
                m => m.ResetPasswordAsync(It.IsAny<Identity.Data.Entities.ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task Handle_InvalidToken_ReturnsFailedResponse()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ResetPasswordAsync(user, "bad-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token." }));

            var result = await _handler.Handle(BuildCommand(token: "bad-token"), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.False);
                Assert.That(result.Message, Is.EqualTo("Password reset failed. The link may be invalid or expired."));
            }
        }

        [Test]
        public async Task Handle_Success_ReturnsSuccessResponse()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ResetPasswordAsync(user, "valid-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            var result = await _handler.Handle(BuildCommand(), CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result!.Succeeded, Is.True);
                Assert.That(result.Message, Is.EqualTo("Your password has been reset successfully."));
            }
        }

        [Test]
        public async Task Handle_Success_CallsResetPasswordWithCorrectArguments()
        {
            var user = new Identity.Data.Entities.ApplicationUser { Id = "user-id" };

            _userManagerMock
                .Setup(m => m.FindByIdAsync("user-id"))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(m => m.ResetPasswordAsync(user, "valid-token", "NewPass123!"))
                .ReturnsAsync(IdentityResult.Success);

            await _handler.Handle(BuildCommand(), CancellationToken.None);

            _userManagerMock.Verify(
                m => m.ResetPasswordAsync(user, "valid-token", "NewPass123!"),
                Times.Once);
        }

        private static ResetPasswordCommand BuildCommand(
            string userId = "user-id",
            string token = "valid-token",
            string newPassword = "NewPass123!") =>
            new()
            {
                Model = new ResetPasswordModel
                {
                    UserId = userId,
                    Token = token,
                    NewPassword = newPassword,
                    ConfirmPassword = newPassword
                }
            };
    }
}
